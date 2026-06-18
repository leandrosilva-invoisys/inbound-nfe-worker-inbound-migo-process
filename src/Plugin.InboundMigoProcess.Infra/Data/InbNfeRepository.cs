using Dapper;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using invoisys.SDK.Tenant.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Plugin.InboundMigoProcess.Infra.Data;

public sealed class InbNfeRepository : IInbNfeRepository
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<InbNfeRepository> _logger;

    public InbNfeRepository(ITenantService tenantService, ILogger<InbNfeRepository> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<long?> ObterDocIdPorInbNfeTagAsync(long subscriptionId, long tagId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT doc_id
            FROM public.inb_nfe_tag
            WHERE subscription_id = @SubscriptionId AND id = @TagId
            LIMIT 1;
            """;

        try
        {
            await using var conn = await _tenantService.GetCdConnectionBySubscription(subscriptionId).ConfigureAwait(false);
            return await conn.QueryFirstOrDefaultAsync<long?>(
                new CommandDefinition(sql, new { SubscriptionId = subscriptionId, TagId = tagId }, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter doc_id em inb_nfe_tag (subscription={SubscriptionId}, inb_nfe_tag.id={TagId}).", subscriptionId, tagId);
            return null;
        }
    }

    public async Task<long?> ObterDocId20Async(long subscriptionId, long docId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT doc_id_20
            FROM public.inb_nfe
            WHERE subscription_id = @SubscriptionId AND id = @DocId
            LIMIT 1;
            """;

        try
        {
            await using var conn = await _tenantService.GetCdConnectionBySubscription(subscriptionId).ConfigureAwait(false);
            return await conn.QueryFirstOrDefaultAsync<long?>(
                new CommandDefinition(sql, new { SubscriptionId = subscriptionId, DocId = docId }, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter doc_id_20 de inb_nfe (subscription={SubscriptionId}, docId={DocId}).", subscriptionId, docId);
            return null;
        }
    }

    public async Task<InboundNfeResumoDto?> ObterResumoNfeAsync(long subscriptionId, long docId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                CONCAT_WS(
                    ' - ',
                    NULLIF(COALESCE(n.xml->'infNFe'->'emit'->>'IE', ''), ''),
                    NULLIF(COALESCE(n.xml->'infNFe'->'emit'->>'xNome', ''), '')
                ) AS Fornecedor,
                CONCAT(COALESCE(n.numero::text, ''), '-', COALESCE(n.serie::text, '')) AS NotaFiscal
            FROM public.inb_nfe n
            WHERE n.subscription_id = @SubscriptionId AND n.id = @DocId
            LIMIT 1;
            """;

        try
        {
            await using var conn = await _tenantService.GetCdConnectionBySubscription(subscriptionId).ConfigureAwait(false);
            return await conn.QueryFirstOrDefaultAsync<InboundNfeResumoDto>(
                new CommandDefinition(
                    sql,
                    new { SubscriptionId = subscriptionId, DocId = docId },
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao obter fornecedor/nota fiscal em inb_nfe (subscription={SubscriptionId}, docId={DocId}).",
                subscriptionId,
                docId);
            return null;
        }
    }
}
