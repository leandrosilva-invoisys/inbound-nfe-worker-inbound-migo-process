// Implementação: extrai o payload da mensagem e delega ao caso de uso.

using System.Text.Json;
using HelloWorld.Application.UseCases.ProcessHelloWorld;
using HelloWorld.Infra.Worker.Model;
using HelloWorld.Infra.Worker.Telemetry;
using Serilog;
using Serilog.Context;
using Traceability;

namespace HelloWorld.Infra.Worker.Services;

public class HelloWorldProcessService : IHelloWorldProcessService
{
    private readonly IProcessHelloWorldUseCase _processHelloWorldUseCase;

    public HelloWorldProcessService(IProcessHelloWorldUseCase processHelloWorldUseCase)
    {
        _processHelloWorldUseCase = processHelloWorldUseCase;
    }

    public async Task<bool> Handle(QueueMessageDTO messageDto)
    {
        var logId = Guid.NewGuid();
        CorrelationContext.Current = logId.ToString("N");
        using var activity = HelloWorldActivitySource.Source.StartActivity("Consumer.Consume");

        using (LogContext.PushProperty("LogId", logId))
        {
            try
            {
                var body = messageDto.Body;
                if (string.IsNullOrWhiteSpace(body))
                {
                    Log.Warning("Mensagem da fila sem Body. Ignorando.");
                    return true; // Remove da fila para não reprocessar mensagem inválida.
                }

                var payload = JsonSerializer.Deserialize<HelloWorldMessagePayload>(body);
                if (payload == null || payload.Id <= 0)
                {
                    Log.Warning("Payload inválido ou Id ausente. Body: {Body}", body);
                    return true;
                }

                var input = new ProcessHelloWorldInput(payload.Id);
                var result = await _processHelloWorldUseCase.Execute(input, logId, CancellationToken.None);
                return result.Sucesso;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao processar mensagem da fila HelloWorld");
                return false; // Permite reprocessar.
            }
        }
    }

    private sealed class HelloWorldMessagePayload
    {
        public int Id { get; set; }
    }
}
