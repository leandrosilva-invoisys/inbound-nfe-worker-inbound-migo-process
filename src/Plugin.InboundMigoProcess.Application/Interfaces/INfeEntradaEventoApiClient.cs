namespace Plugin.InboundMigoProcess.Application.Interfaces;

public interface INfeEntradaEventoApiClient
{
    Task<bool> AdicionarEventoVinculoAsync(int nfeEntradaId, string descricao, CancellationToken cancellationToken);
}
