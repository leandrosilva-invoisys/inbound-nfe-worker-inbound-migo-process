using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Plugin.InboundMigoProcess.Application.Audit.Command;
using Plugin.InboundMigoProcess.Application.Audit.DTO;
using Plugin.InboundMigoProcess.Application.Audit.Enum;
using Plugin.InboundMigoProcess.Application.Audit.Interface;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Plugin.InboundMigoProcess.Application.Services;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;
using Microsoft.Extensions.Logging;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

public sealed class ProcessInboundMigoReverseUseCase : IProcessInboundMigoReverseUseCase
{
    public const string TransactionTypeReverse = "inbound.sync.migo.reverse";
    private const string TagInboundMigo = "inbound.migo";
    private const string TagInboundMigoCancelamento = "inbound.migo.cancelamento";

    public string TransactionType => TransactionTypeReverse;

    private readonly IInboundTransactionRepository _transactionRepository;
    private readonly IInbNfeRepository _inbNfeRepository;
    private readonly ITagManagerApiClient _tagManagerApiClient;
    private readonly ITransactionResultMapper _transactionResultMapper;
    private readonly ISapMessageEvaluator _sapMessageEvaluator;
    private readonly ITagValueBuilder _tagValueBuilder;
    private readonly IAuditAppService _auditAppService;
    private readonly INfeEntradaEventoApiClient _nfeEntradaEventoApiClient;
    private readonly ILogger<ProcessInboundMigoReverseUseCase> _logger;

    public ProcessInboundMigoReverseUseCase(
        IInboundTransactionRepository transactionRepository,
        IInbNfeRepository inbNfeRepository,
        ITagManagerApiClient tagManagerApiClient,
        ITransactionResultMapper transactionResultMapper,
        ISapMessageEvaluator sapMessageEvaluator,
        ITagValueBuilder tagValueBuilder,
        IAuditAppService auditAppService,
        INfeEntradaEventoApiClient nfeEntradaEventoApiClient,
        ILogger<ProcessInboundMigoReverseUseCase> logger)
    {
        _transactionRepository = transactionRepository;
        _inbNfeRepository = inbNfeRepository;
        _tagManagerApiClient = tagManagerApiClient;
        _transactionResultMapper = transactionResultMapper;
        _sapMessageEvaluator = sapMessageEvaluator;
        _tagValueBuilder = tagValueBuilder;
        _auditAppService = auditAppService;
        _nfeEntradaEventoApiClient = nfeEntradaEventoApiClient;
        _logger = logger;
    }

    public Task<ProcessInboundMigoReverseOutput> ExecuteAsync(
        ProcessInboundMigoReverseInput input,
        CancellationToken cancellationToken)
    {
        return ExecuteInternalAsync(input, cancellationToken);
    }

    public async Task<UseCaseOutput> ExecuteAsync(string transactionId, CancellationToken cancellationToken)
    {
        var input = new ProcessInboundMigoReverseInput(transactionId);
        return await ExecuteAsync(input, cancellationToken);
    }

    private async Task<ProcessInboundMigoReverseOutput> ExecuteInternalAsync(
        ProcessInboundMigoReverseInput input,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.ObterPorTransactionIdAsync(input.TransactionId, cancellationToken);
        if (transaction is null)
            return Falha($"Transação {input.TransactionId} não encontrada.");

        if (!string.Equals(transaction.TransactionType, TransactionTypeReverse, StringComparison.OrdinalIgnoreCase))
        {
            return new ProcessInboundMigoReverseOutput
            {
                Sucesso = true,
                Mensagem = $"TransactionType {transaction.TransactionType} ignorado para este worker."
            };
        }

        long? docId = transaction.DocId;
        if ((!docId.HasValue || docId == 0) && transaction.TagId.HasValue)
            docId = await _inbNfeRepository.ObterDocIdPorInbNfeTagAsync(transaction.SubscriptionId, transaction.TagId.Value, cancellationToken);

        if (!docId.HasValue)
            return Falha("DocId não encontrado para processar o estorno MIGO.");

        var inboundMigoTag = await _tagManagerApiClient.ObterTagAsync(
            transaction.SubscriptionId,
            docId.Value,
            TagInboundMigo,
            cancellationToken);
        if (inboundMigoTag is null)
            return Falha("Tag inbound.migo não encontrada.");

        var cancelamentoTag = await _tagManagerApiClient.ObterTagAsync(
            transaction.SubscriptionId,
            docId.Value,
            TagInboundMigoCancelamento,
            cancellationToken);
        if (cancelamentoTag is null)
            return Falha("Tag inbound.migo.cancelamento não encontrada.");

        var tagValue = ParseInboundMigoTag(inboundMigoTag.TagValue);
        var materialDocument = tagValue.MaterialDocument ?? string.Empty;
        var matDocumentYear = tagValue.MatDocumentYear ?? string.Empty;

        var result = _transactionResultMapper.Map(transaction.ResultRaw);
        var messages = result.Return;
        var sucesso = _sapMessageEvaluator.PossuiSucesso(messages);
        var idempotente = _sapMessageEvaluator.EhIdempotenciaJaEstornado(messages);
        var erroGenerico = _sapMessageEvaluator.PossuiErro(messages) && !idempotente;

        var mensagemPrincipal = messages.FirstOrDefault();
        var messageV1 = mensagemPrincipal?.MessageV1 ?? string.Empty;
        var messageV2 = mensagemPrincipal?.MessageV2 ?? string.Empty;
        var resultado = erroGenerico ? "erro" : idempotente ? "idempotencia" : "sucesso";

        var updateCancelamentoOk = await _tagManagerApiClient.AtualizarTagAsync(new TagManagerRequestUpdate
        {
            SubscriptionId = transaction.SubscriptionId,
            Id = cancelamentoTag.Id,
            Status = erroGenerico ? 3 : 2,
            TagValue = _tagValueBuilder.BuildInboundMigoCancelamentoTagValue(
                transaction.TransactionId.ToString(),
                materialDocument,
                matDocumentYear,
                resultado,
                messages)
        }, cancellationToken);

        if (!updateCancelamentoOk)
            return Falha("Erro ao atualizar inbound.migo.cancelamento.");

        if (!erroGenerico)
        {
            var updateMigoOk = await _tagManagerApiClient.AtualizarTagAsync(new TagManagerRequestUpdate
            {
                SubscriptionId = transaction.SubscriptionId,
                Id = inboundMigoTag.Id,
                Status = 3,
                TagValue = _tagValueBuilder.BuildInboundMigoTagValueLimpo(inboundMigoTag.TagValue, removerCamposMovimentacao: true)
            }, cancellationToken);

            if (!updateMigoOk)
                return Falha("Erro ao atualizar inbound.migo.");
        }

        await PublicarAuditoriaAsync(
            transaction,
            materialDocument,
            matDocumentYear,
            resultado,
            messageV1,
            messageV2,
            messages,
            docId.Value);

        await RegistrarEventoAsync(
            transaction,
            docId.Value,
            materialDocument,
            messageV1,
            messageV2,
            erroGenerico,
            idempotente,
            cancellationToken);

        return new ProcessInboundMigoReverseOutput
        {
            Sucesso = true,
            Mensagem = "Retorno de estorno MIGO processado com sucesso."
        };
    }

    private async Task PublicarAuditoriaAsync(
        InboundTransactionRecordDto transaction,
        string materialDocument,
        string matDocumentYear,
        string resultado,
        string messageV1,
        string messageV2,
        List<SapReturnMessageDto> messages,
        long docId)
    {
        try
        {
            var dataHora = DateTime.Now;
            var usuario = string.IsNullOrWhiteSpace(transaction.OriginatorService)
                ? "Usuário não identificado"
                : transaction.OriginatorService;

            var auditData = new
            {
                dataHoraEstorno = dataHora,
                materialDocument,
                matDocumentYear,
                transactionId = transaction.TransactionId.ToString(),
                resultado,
                messageV1,
                messageV2,
                RETURN = messages,
                usuarioSolicitante = usuario
            };

            var empresas = new List<AuditEmpresaDTO> { new() { cnpj = string.Empty, apelido = string.Empty } };
            var auditRequest = AuditCommand.CreateAuditInsert(
                empresas,
                AuditContextEnum.NFeRecebida,
                AuditFunctionEnum.EditarCadastro,
                docId.ToString(CultureInfo.InvariantCulture),
                auditData);

            var normalizedUserId = usuario.Replace(" ", ".", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
            auditRequest.SetUser(
                transaction.SubscriptionId.ToString(CultureInfo.InvariantCulture),
                normalizedUserId,
                $"{normalizedUserId}@invoisys.com.br",
                usuario);

            auditRequest.SetDiffList(
                ("MaterialDocument", materialDocument, materialDocument),
                ("MatDocumentYear", matDocumentYear, matDocumentYear),
                ("TransactionId", transaction.TransactionId.ToString(), transaction.TransactionId.ToString()),
                ("Resultado", resultado, resultado));

            await _auditAppService.HandleAsync(auditRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar auditoria do estorno MIGO.");
        }
    }

    private async Task RegistrarEventoAsync(
        InboundTransactionRecordDto transaction,
        long docId,
        string materialDocument,
        string messageV1,
        string messageV2,
        bool erroGenerico,
        bool idempotente,
        CancellationToken cancellationToken)
    {
        try
        {
            var docId20 = await _inbNfeRepository.ObterDocId20Async(transaction.SubscriptionId, docId, cancellationToken);
            if (!docId20.HasValue || docId20.Value is < int.MinValue or > int.MaxValue)
                return;

            var descricao = erroGenerico
                ? $"Erro no estorno da Movimentação de Estoque {materialDocument}."
                : idempotente
                    ? $"Estorno da Movimentação de Estoque {materialDocument} — documento já estava estornado no SAP. Tag atualizada e número apagado do sistema."
                    : $"Estorno da Movimentação de Estoque {materialDocument} realizado com sucesso. Documento de estorno: {messageV1}/{messageV2}. Número da movimentação apagado do sistema.";

            await _nfeEntradaEventoApiClient.AdicionarEventoVinculoAsync((int)docId20.Value, descricao, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar evento de estorno MIGO.");
        }
    }

    private static ProcessInboundMigoReverseOutput Falha(string mensagem)
    {
        return new ProcessInboundMigoReverseOutput
        {
            Sucesso = false,
            Mensagem = mensagem
        };
    }

    private static InboundMigoTagValue ParseInboundMigoTag(string? rawTagValue)
    {
        if (string.IsNullOrWhiteSpace(rawTagValue))
            return new InboundMigoTagValue();

        try
        {
            using var doc = JsonDocument.Parse(rawTagValue);
            var root = doc.RootElement;
            return new InboundMigoTagValue
            {
                MaterialDocument = GetString(root, "materialdocument", "MATERIALDOCUMENT"),
                MatDocumentYear = GetString(root, "matdocumentyear", "MATDOCUMENTYEAR")
            };
        }
        catch (JsonException)
        {
            return new InboundMigoTagValue();
        }
    }

    private static string? GetString(JsonElement root, string lower, string upper)
    {
        if (root.TryGetProperty(lower, out var lowerValue) && lowerValue.ValueKind == JsonValueKind.String)
            return lowerValue.GetString();
        if (root.TryGetProperty(upper, out var upperValue) && upperValue.ValueKind == JsonValueKind.String)
            return upperValue.GetString();
        return null;
    }

    private sealed class InboundMigoTagValue
    {
        public string? MaterialDocument { get; init; }
        public string? MatDocumentYear { get; init; }
    }
}
