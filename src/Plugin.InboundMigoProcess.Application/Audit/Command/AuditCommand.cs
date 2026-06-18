using Plugin.InboundMigoProcess.Application.Audit.DTO;
using Plugin.InboundMigoProcess.Application.Audit.Enum;

namespace Plugin.InboundMigoProcess.Application.Audit.Command;

public sealed class AuditCommand
{
    private AuditCommand(
        AuditContextEnum contextInput,
        string contextIdInput,
        AuditActionEnum actionInput,
        AuditFunctionEnum functionInput,
        object? modelAfterInput,
        List<AuditEmpresaDTO>? empresasInput = null)
    {
        transactionId = Guid.NewGuid().ToString();
        timestamp = DateTime.Now;
        context = contextInput;
        contextId = contextIdInput;
        action = actionInput;
        function = functionInput;
        empresas = empresasInput ?? [];
        modelAfter = modelAfterInput;
        diffList = BuildDiffList([("N/A", null, null)]);
    }

    public string transactionId { get; private set; } = string.Empty;
    public string subscriptionId { get; private set; } = string.Empty;
    public string indexName { get; private set; } = string.Empty;
    public DateTime timestamp { get; private set; }
    public string userId { get; private set; } = string.Empty;
    public string userEmail { get; private set; } = string.Empty;
    public string userName { get; private set; } = string.Empty;
    public List<AuditEmpresaDTO> empresas { get; private set; } = [];
    public AuditContextEnum context { get; private set; }
    public string contextId { get; private set; } = string.Empty;
    public AuditActionEnum action { get; private set; }
    public AuditFunctionEnum function { get; private set; }
    public string keyDataBefore { get; private set; } = string.Empty;
    public string keyDataAfter { get; private set; } = string.Empty;
    public string logDetail { get; private set; } = string.Empty;
    public object? modelBefore { get; private set; }
    public object? modelAfter { get; private set; }
    public object? diffList { get; private set; }

    public static AuditCommand CreateAuditInsert(
        List<AuditEmpresaDTO> empresas,
        AuditContextEnum context,
        AuditFunctionEnum function,
        string contextId,
        object modelAfterInput)
    {
        return new AuditCommand(context, contextId, AuditActionEnum.insert, function, modelAfterInput, empresas);
    }

    public void SetUser(string subscription, string userIdInput, string userEmailInput, string userNameInput)
    {
        subscriptionId = subscription;
        userId = userIdInput;
        userEmail = userEmailInput;
        userName = userNameInput;
    }

    public void SetDiffList(params (string Propriedade, object? De, object? Para)[] itens)
    {
        if (itens is null || itens.Length == 0)
            return;
        diffList = BuildDiffList(itens);
    }

    public void SetEnv(int env)
    {
        indexName = env == 1 ? "prd_audittrail" : "sbx_audittrail";
    }

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(contextId))
            return false;
        return empresas.Count > 0;
    }

    private static List<object> BuildDiffList(IEnumerable<(string Propriedade, object? De, object? Para)> itens)
    {
        return itens.Select(item => new { item.Propriedade, item.De, item.Para }).Cast<object>().ToList();
    }
}
