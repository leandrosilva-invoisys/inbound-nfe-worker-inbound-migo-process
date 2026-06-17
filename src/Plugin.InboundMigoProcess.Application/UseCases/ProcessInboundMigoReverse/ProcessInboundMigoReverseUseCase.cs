using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

public sealed class ProcessInboundMigoReverseUseCase : IProcessInboundMigoReverseUseCase
{
    public const string TransactionTypeReverse = "inbound.sync.migo.reverse";

    public string TransactionType => TransactionTypeReverse;

    public Task<ProcessInboundMigoReverseOutput> ExecuteAsync(
        ProcessInboundMigoReverseInput input,
        CancellationToken cancellationToken)
    {
        var output = new ProcessInboundMigoReverseOutput
        {
            Sucesso = true,
            Mensagem = $"Use case '{nameof(ProcessInboundMigoReverseUseCase)}' inicializado sem lógica de negócio."
        };

        return Task.FromResult(output);
    }

    public async Task<UseCaseOutput> ExecuteAsync(string transactionId, CancellationToken cancellationToken)
    {
        var input = new ProcessInboundMigoReverseInput(transactionId);
        return await ExecuteAsync(input, cancellationToken);
    }
}
