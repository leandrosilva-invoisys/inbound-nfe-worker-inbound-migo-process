using Plugin.InboundMigoProcess.Infra.Worker.Model;
using Plugin.InboundMigoProcess.Infra.Worker.Services;
using invoisys.SDK.Queue.Attributes;
using invoisys.SDK.Queue.Interfaces;

namespace Plugin.InboundMigoProcess.Infra.Worker;

[QueueName("0118-inbound-nfe-worker-inbound-migo-process")]
public class Consumer : IConsumer<QueueMessageDTO>
{
    private readonly IInboundMigoProcessService _processService;

    public Consumer(IInboundMigoProcessService processService)
    {
        _processService = processService;
    }

    public async Task<bool> Consume(QueueMessageDTO messageDto)
    {
        var resultado = await _processService.Handle(messageDto);
        return resultado;
    }
}
