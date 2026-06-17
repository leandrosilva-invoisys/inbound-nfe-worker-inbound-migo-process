using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;

public interface IProcessInboundMigoConfirmUseCase
    : IProcessInboundMigoUseCase<ProcessInboundMigoConfirmInput, ProcessInboundMigoConfirmOutput>
{
}
