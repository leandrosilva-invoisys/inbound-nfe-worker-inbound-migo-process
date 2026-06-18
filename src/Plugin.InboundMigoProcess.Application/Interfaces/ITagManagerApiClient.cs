using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Interfaces;

public interface ITagManagerApiClient
{
    Task<TagManagerTagDto?> ObterTagAsync(long subscriptionId, long docId, string tagName, CancellationToken cancellationToken);
    Task<IReadOnlyList<TagManagerTagDto>> ObterTagsAsync(long subscriptionId, long docId, string tagName, CancellationToken cancellationToken);
    Task<bool> AtualizarTagAsync(TagManagerRequestUpdate request, CancellationToken cancellationToken);
    Task<long?> CriarTagAsync(TagManagerRequestAdd request, CancellationToken cancellationToken);
    Task<bool> RemoverTagAsync(TagManagerRequestRemove request, CancellationToken cancellationToken);
}
