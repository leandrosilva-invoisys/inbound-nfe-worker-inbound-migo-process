using Plugin.InboundMigoProcess.Application.Interfaces;

namespace Plugin.InboundMigoProcess.Infra.Data;

public sealed class InboundTransactionRepository : IInboundTransactionRepository
{
    public Task<string?> ObterTransactionTypePorTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return Task.FromResult<string?>(null);

        if (transactionId.Contains("reverse", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>("inbound.sync.migo.reverse");

        if (transactionId.Contains("confirm", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>("inbound.sync.inbound-delivery.confirm");

        return Task.FromResult<string?>("inbound.sync.inbound-delivery.confirm");
    }
}
