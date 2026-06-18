using Dapper;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using invoisys.SDK.SecretManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Plugin.InboundMigoProcess.Infra.Data;

public sealed class InboundTransactionRepository : IInboundTransactionRepository
{
    private readonly ILogger<InboundTransactionRepository> _logger;
    private readonly Lazy<Task<string>> _connectionStringLazy;

    public InboundTransactionRepository(
        ISecretService secretService,
        IConfiguration configuration,
        ILogger<InboundTransactionRepository> logger)
    {
        _logger = logger;
        _connectionStringLazy = new Lazy<Task<string>>(() =>
            PostgreSqlCd30ConnectionResolver.ResolveAsync(configuration, secretService));
    }

    public async Task<InboundTransactionRecordDto?> ObterPorTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return null;

        if (!Guid.TryParse(transactionId, out var transactionGuid))
        {
            _logger.LogWarning("TransactionId inválido: {TransactionId}", transactionId);
            return null;
        }

        const string sql = """
            SELECT
                transaction_id AS TransactionId,
                transactiontype AS TransactionType,
                subscription_id AS SubscriptionId,
                docid AS DocId,
                originatorservice AS OriginatorService,
                tag_id AS TagId,
                result AS ResultRaw
            FROM public."invs_transactionControl_inbound"
            WHERE transaction_id = @TransactionId
            LIMIT 1;
        """;

        await using var conn = new NpgsqlConnection(await _connectionStringLazy.Value.ConfigureAwait(false));
        return await conn.QueryFirstOrDefaultAsync<InboundTransactionRecordDto>(
            new CommandDefinition(sql, new { TransactionId = transactionGuid }, cancellationToken: cancellationToken));
    }

    public async Task<string?> ObterTransactionTypePorTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(transactionId, out var transactionGuid))
        {
            _logger.LogWarning("TransactionId inválido: {TransactionId}", transactionId);
            return null;
        }

        const string sql = """
            SELECT transactiontype
            FROM public."invs_transactionControl_inbound"
            WHERE transaction_id = @TransactionId
            LIMIT 1;
        """;

        await using var conn = new NpgsqlConnection(await _connectionStringLazy.Value.ConfigureAwait(false));
        return await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition(sql, new { TransactionId = transactionGuid }, cancellationToken: cancellationToken));
    }
}
