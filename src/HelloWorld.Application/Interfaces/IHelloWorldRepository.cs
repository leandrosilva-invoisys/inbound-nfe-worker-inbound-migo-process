// Substitua HelloWorld pelo nome do seu domínio (ex: IItemRepository, IPedidoGateway).

namespace HelloWorld.Application.Interfaces;

public interface IHelloWorldRepository
{
    /// <summary>
    /// Obtém os identificadores dos itens a serem processados pelo worker.
    /// Substitua pelo contrato real do seu projeto (ex: ObterDocsParaEpecAsync, ObterPedidosPendentesAsync).
    /// </summary>
    Task<List<int>> ObterIdsParaProcessarAsync();

    /// <summary>
    /// Obtém dado adicional por id, se necessário (ex: nome de arquivo, payload).
    /// Ajuste a assinatura conforme sua regra de negócio.
    /// </summary>
    Task<string> ObterDadoPorIdAsync(int id);
}
