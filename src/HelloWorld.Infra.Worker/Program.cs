// Host do worker. Registre aqui os serviços (HttpClient, Repository, UseCase, HostedService).
// Substitua HelloWorld pelo nome do seu projeto.

using System.Reflection;
using HelloWorld.Application.Interfaces;
using HelloWorld.Application.UseCases.ProcessHelloWorld;
using HelloWorld.Infra.ApiClients;
using HelloWorld.Infra.Data;
using HelloWorld.Infra.Worker.Logging;
using HelloWorld.Infra.Worker.Services;
using HelloWorld.Infra.Worker.Telemetry;
using invoisys.SDK.Host.Helpers;
using invoisys.SDK.Queue.Extensions;
using invoisys.SDK.SecretManager.Extensions;
using invoisys.SDK.Storage.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Traceability.Extensions;
using Traceability.Logging;

// Serilog com Correlation-Id e TraceId (X-Trace-Id) em todos os logs.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.With<CorrelationIdEnricher>()
    .Enrich.With<TraceIdEnricher>()
    .Enrich.WithProperty("Application", "HelloWorldWorker")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] CorrelationId={CorrelationId} TraceId={TraceId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var serviceName = "HelloWorldWorker";
var versionFromConfig = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion ?? "1.0.0";

var app = WorkerHostHelpers.BuildHost(services =>
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    });

    services.AddTraceability(serviceName);

    var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    var enableMetricsConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:EnableMetricsConsoleExporter", true);
    var isDevelopment = configuration.GetValue<string>("DOTNET_ENVIRONMENT") == "Development"
        || configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

    var openTelemetryBuilder = services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName, serviceVersion: versionFromConfig))
        .WithTracing(tracing =>
        {
            tracing
                .AddSource(HelloWorldActivitySource.Source.Name)
                .AddHttpClientInstrumentation()
                .AddConsoleExporter();

            if (!isDevelopment)
            {
                var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            }
        })
        .WithMetrics(metrics =>
        {
            metrics.AddHttpClientInstrumentation();
            if (enableMetricsConsoleExporter)
                metrics.AddConsoleExporter();
            if (!isDevelopment)
            {
                var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            }
        });

    // HttpClient para o API client. Ajuste BaseAddress e timeout conforme sua API.
    services.AddHttpClient<IHelloWorldApiClient, HelloWorldApiClient>(client =>
    {
        client.BaseAddress = new Uri(configuration.GetValue<string>("HelloWorld:BaseAddress") ?? "https://localhost");
        client.Timeout = TimeSpan.FromMinutes(1);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    services.AddSecretManagerServices();
    services.AddStorageServices();

    services.AddScoped<IHelloWorldRepository, HelloWorldRepository>();
    services.AddScoped<IProcessHelloWorldUseCase, ProcessHelloWorldUseCase>();
    services.AddScoped<IHelloWorldProcessService, HelloWorldProcessService>();

    services.AddSQSQueue();

    services.AddHostedService<HelloWorld.Infra.Worker.Worker>();
});

app.Run();
