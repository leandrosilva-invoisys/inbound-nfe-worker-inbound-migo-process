using System.Reflection;
using invoisys.SDK.Host.Helpers;
using invoisys.SDK.Queue.Extensions;
using invoisys.SDK.SecretManager.Extensions;
using invoisys.SDK.Storage.Extensions;
using invoisys.SDK.Tenant.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plugin.InboundMigoProcess.Application.Audit.Interface;
using Plugin.InboundMigoProcess.Application.Audit.Service;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Plugin.InboundMigoProcess.Application.Services;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;
using Plugin.InboundMigoProcess.Infra.ApiClients;
using Plugin.InboundMigoProcess.Infra.Data;
using Plugin.InboundMigoProcess.Infra.Worker;
using Plugin.InboundMigoProcess.Infra.Worker.Logging;
using Plugin.InboundMigoProcess.Infra.Worker.Services;
using Plugin.InboundMigoProcess.Infra.Worker.Telemetry;
using Serilog;
using Traceability.Extensions;
using Traceability.Logging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.With<CorrelationIdEnricher>()
    .Enrich.With<TraceIdEnricher>()
    .Enrich.WithProperty("Application", "InboundMigoProcessWorker")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] CorrelationId={CorrelationId} TraceId={TraceId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var serviceName = "InboundMigoProcessWorker";
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
    var enableMetricsConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:EnableMetricsConsoleExporter", false);
    var enableTracingConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:EnableTracingConsoleExporter", false);
    var isDevelopment = configuration.GetValue<string>("DOTNET_ENVIRONMENT") == "Development"
        || configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName, serviceVersion: versionFromConfig))
        .WithTracing(tracing =>
        {
            tracing
                .AddSource(InboundMigoProcessActivitySource.Source.Name)
                .AddHttpClientInstrumentation();

            if (enableTracingConsoleExporter)
                tracing.AddConsoleExporter();

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

    services.AddSecretManagerServices();
    services.AddTenantServices();
    services.AddStorageServices();

    services.AddHttpClient<ITagManagerApiClient, TagManagerApiClient>(client =>
    {
        client.BaseAddress = new Uri(configuration["TagManager:BaseAddress"]
            ?? throw new InvalidOperationException("TagManager:BaseAddress não configurado."));
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    services.AddHttpClient<INfeEntradaEventoApiClient, NfeEntradaEventoApiClient>(client =>
    {
        var baseUrl = configuration["NfeEntradaApi:BaseAddress"]
            ?? throw new InvalidOperationException("NfeEntradaApi:BaseAddress não configurado.");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    services.AddScoped<IInboundTransactionRepository, InboundTransactionRepository>();
    services.AddScoped<IInbNfeRepository, InbNfeRepository>();
    services.AddScoped<ITransactionResultMapper, TransactionResultMapper>();
    services.AddScoped<ISapMessageEvaluator, SapMessageEvaluator>();
    services.AddScoped<ITagValueBuilder, TagValueBuilder>();
    services.AddScoped<IAuditAppService, AuditAppService>();
    services.AddScoped<IProcessInboundMigoConfirmUseCase, ProcessInboundMigoConfirmUseCase>();
    services.AddScoped<IProcessInboundMigoReverseUseCase, ProcessInboundMigoReverseUseCase>();
    services.AddScoped<IProcessInboundMigoUseCase>(sp => sp.GetRequiredService<IProcessInboundMigoConfirmUseCase>());
    services.AddScoped<IProcessInboundMigoUseCase>(sp => sp.GetRequiredService<IProcessInboundMigoReverseUseCase>());
    services.AddScoped<IInboundMigoUseCaseFactory, InboundMigoUseCaseFactory>();
    services.AddScoped<IInboundMigoProcessService, InboundMigoProcessService>();

    services.AddSQSQueue();
});

app.Run();
