namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

public interface IInboundMigoUseCaseFactory
{
    IProcessInboundMigoUseCase? Resolve(string? transactionType);
}
