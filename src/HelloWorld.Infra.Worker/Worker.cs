using System.Diagnostics;
using HelloWorld.Application.Interfaces;
using HelloWorld.Application.UseCases.ProcessHelloWorld;
using HelloWorld.Infra.Worker.Telemetry;
using Serilog;
using Serilog.Context;
using Traceability;

namespace HelloWorld.Infra.Worker;

/// <summary>
/// Template de worker que executa um job em loop em intervalos configuráveis.
/// Use este arquivo quando for necessário rodar alguma tarefa de tempos em tempos: jobs agendados,
/// processamento em lote (batch), varreduras periódicas, sincronizações, envio de notificações, etc.
/// </summary>
/// <remarks>
/// <para><b>Quando usar:</b> jobs que rodam periodicamente (ex.: a cada X minutos/horas), sem depender de fila — o próprio worker "acorda" e executa a lógica.</para>
/// <para><b>Como adaptar:</b></para>
/// <list type="bullet">
///   <item><b>Intervalo:</b> altere o <c>Task.Delay</c> no final do loop (ex.: <c>TimeSpan.FromMinutes(15)</c>, <c>TimeSpan.FromHours(1)</c>).</item>
///   <item><b>Lógica:</b> substitua a obtenção de itens (<c>repository.ObterIdsParaProcessarAsync</c>) e o use case (<c>IProcessHelloWorldUseCase</c>) pelos da sua regra de negócio.</item>
///   <item><b>Escopo:</b> use <c>CreateScope()</c> para resolver serviços com escopo (DbContext, repositórios) a cada iteração, evitando uso em longo prazo.</item>
///   <item><b>Cancelação:</b> respeite <c>stoppingToken</c> no Delay e nas operações assíncronas para desligamento graceful do host.</item>
/// </list>
/// </remarks>
public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IHelloWorldRepository>();
        var useCase = scope.ServiceProvider.GetRequiredService<IProcessHelloWorldUseCase>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var logId = Guid.NewGuid();
            CorrelationContext.Current = logId.ToString("N");
            using var activity = HelloWorldActivitySource.Source.StartActivity("Worker.ExecuteAsync");

            using (LogContext.PushProperty("LogId", logId))
            {
                try
                {
                    Log.Information("Iniciando execução do Worker HelloWorld");

                    var ids = await repository.ObterIdsParaProcessarAsync();
                    Log.Information("Encontrados {Total} itens para processar", ids.Count);

                    if (ids.Count == 0)
                    {
                        Log.Information("Nenhum item encontrado. Aguardando próxima execução.");
                    }
                    else
                    {
                        var sucesso = 0;
                        var falha = 0;
                        foreach (var id in ids)
                        {
                            Log.Information("Processando Id: {Id}", id);
                            var resultado = await useCase.Execute(new ProcessHelloWorldInput(id), logId, stoppingToken);
                            if (resultado.Sucesso)
                                sucesso++;
                            else
                                falha++;
                        }
                        Log.Information("Execução finalizada. Sucesso: {Sucesso}, Falha: {Falha}", sucesso, falha);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro crítico no Worker HelloWorld");
                    throw;
                }
            }

            // TODO: Ajustar intervalo (ex: 15 minutos para EPEC).
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
