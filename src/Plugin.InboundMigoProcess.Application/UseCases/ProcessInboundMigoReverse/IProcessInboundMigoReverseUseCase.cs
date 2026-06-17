using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

public interface IProcessInboundMigoReverseUseCase
    : IProcessInboundMigoUseCase<ProcessInboundMigoReverseInput, ProcessInboundMigoReverseOutput>
{
}
