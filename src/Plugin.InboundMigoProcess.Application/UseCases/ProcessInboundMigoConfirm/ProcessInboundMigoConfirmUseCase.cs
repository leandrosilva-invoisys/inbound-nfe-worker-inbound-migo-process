using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;

public sealed class ProcessInboundMigoConfirmUseCase : IProcessInboundMigoConfirmUseCase
{
    public const string TransactionTypeConfirm = "inbound.sync.inbound-delivery.confirm";

    public string TransactionType => TransactionTypeConfirm;

    public Task<ProcessInboundMigoConfirmOutput> ExecuteAsync(
        ProcessInboundMigoConfirmInput input,
        CancellationToken cancellationToken)
    {
        var output = new ProcessInboundMigoConfirmOutput
        {
            Sucesso = true,
            Mensagem = $"Use case '{nameof(ProcessInboundMigoConfirmUseCase)}' inicializado sem lógica de negócio."
        };

        return Task.FromResult(output);
    }

    public async Task<UseCaseOutput> ExecuteAsync(string transactionId, CancellationToken cancellationToken)
    {
        var input = new ProcessInboundMigoConfirmInput(transactionId);
        return await ExecuteAsync(input, cancellationToken);
    }
}
