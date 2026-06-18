using System.Text.Json;
using Plugin.InboundMigoProcess.Application.Audit.Command;
using Plugin.InboundMigoProcess.Application.Audit.Interface;
using invoisys.SDK.Queue.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Plugin.InboundMigoProcess.Application.Audit.Service;

public sealed class AuditAppService : IAuditAppService
{
    private readonly IPublisher _publisher;
    private readonly ILogger<AuditAppService> _logger;
    private readonly string _filaAws;
    private readonly int _ambiente;

    public AuditAppService(
        IPublisher publisher,
        IConfiguration configuration,
        ILogger<AuditAppService> logger)
    {
        _publisher = publisher;
        _logger = logger;
        _filaAws = configuration.GetValue<string>("AWS_metrics_queue")
                   ?? throw new InvalidOperationException("AWS_metrics_queue não configurado.");

        var invoisysEnv = configuration.GetValue<string>("INVOISYS_ENV");
        _ambiente = string.Equals(invoisysEnv, "prd", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
    }

    public async Task HandleAsync(AuditCommand request)
    {
        try
        {
            if (!request.IsValid())
            {
                _logger.LogWarning("Auditoria inválida para contexto {Context} e contextId {ContextId}", request.context, request.contextId);
                return;
            }

            request.SetEnv(_ambiente);
            var payload = JsonSerializer.Serialize(request);
            await _publisher.Publish(_filaAws, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar auditoria. Contexto {Context}, ContextId {ContextId}", request.context, request.contextId);
        }
    }
}
