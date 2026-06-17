namespace Plugin.InboundMigoProcess.Application.Interfaces;

public interface IInboundTransactionRepository
{
    Task<string?> ObterTransactionTypePorTransactionIdAsync(string transactionId, CancellationToken cancellationToken);
}
