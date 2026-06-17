using Plugin.InboundMigoProcess.Application.Interfaces;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;
using Plugin.InboundMigoProcess.Infra.Worker.Model;
using Plugin.InboundMigoProcess.Infra.Worker.Telemetry;
using Serilog;
using Serilog.Context;
using Traceability;

namespace Plugin.InboundMigoProcess.Infra.Worker.Services;

public sealed class InboundMigoProcessService : IInboundMigoProcessService
{
    private readonly IInboundMigoUseCaseFactory _useCaseFactory;
    private readonly IInboundTransactionRepository _inboundTransactionRepository;

    public InboundMigoProcessService(
        IInboundMigoUseCaseFactory useCaseFactory,
        IInboundTransactionRepository inboundTransactionRepository)
    {
        _useCaseFactory = useCaseFactory;
        _inboundTransactionRepository = inboundTransactionRepository;
    }

    public async Task<bool> Handle(QueueMessageDTO messageDto)
    {
        var logId = Guid.NewGuid();
        CorrelationContext.Current = logId.ToString("N");
        using var activity = InboundMigoProcessActivitySource.Source.StartActivity("Consumer.Consume");

        using (LogContext.PushProperty("LogId", logId))
        {
            try
            {
                var transactionId = messageDto.TransactionId;
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    Log.Warning("Payload inválido ou transactionId ausente. Mensagem descartada.");
                    return true;
                }

                var transactionType = await _inboundTransactionRepository
                    .ObterTransactionTypePorTransactionIdAsync(transactionId, CancellationToken.None);

                if (string.IsNullOrWhiteSpace(transactionType))
                {
                    Log.Warning("TransactionType não encontrado para TransactionId {TransactionId}.", transactionId);
                    return true;
                }

                var useCase = _useCaseFactory.Resolve(transactionType);
                if (useCase is null)
                {
                    Log.Information("TransactionType {TransactionType} ignorado por este worker.", transactionType);
                    return true;
                }

                var result = await useCase.ExecuteAsync(transactionId, CancellationToken.None);
                return result.Sucesso;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao processar mensagem da fila InboundMigo.");
                return false;
            }
        }
    }
}
