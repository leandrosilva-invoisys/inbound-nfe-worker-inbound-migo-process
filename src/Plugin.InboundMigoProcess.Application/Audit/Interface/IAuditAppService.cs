using Plugin.InboundMigoProcess.Application.Audit.Command;

namespace Plugin.InboundMigoProcess.Application.Audit.Interface;

public interface IAuditAppService
{
    Task HandleAsync(AuditCommand request);
}
