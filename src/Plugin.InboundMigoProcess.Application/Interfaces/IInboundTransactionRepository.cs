using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Interfaces;

public interface IInboundTransactionRepository
{
    Task<InboundTransactionRecordDto?> ObterPorTransactionIdAsync(string transactionId, CancellationToken cancellationToken);
    Task<string?> ObterTransactionTypePorTransactionIdAsync(string transactionId, CancellationToken cancellationToken);
}
