using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Interfaces;

public interface IInbNfeRepository
{
    Task<long?> ObterDocIdPorInbNfeTagAsync(long subscriptionId, long tagId, CancellationToken cancellationToken);
    Task<long?> ObterDocId20Async(long subscriptionId, long docId, CancellationToken cancellationToken);
    Task<InboundNfeResumoDto?> ObterResumoNfeAsync(long subscriptionId, long docId, CancellationToken cancellationToken);
}
