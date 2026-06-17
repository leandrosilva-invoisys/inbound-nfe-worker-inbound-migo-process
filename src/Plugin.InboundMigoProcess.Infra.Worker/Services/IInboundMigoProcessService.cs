using Plugin.InboundMigoProcess.Infra.Worker.Model;

namespace Plugin.InboundMigoProcess.Infra.Worker.Services;

public interface IInboundMigoProcessService
{
    Task<bool> Handle(QueueMessageDTO messageDto);
}
