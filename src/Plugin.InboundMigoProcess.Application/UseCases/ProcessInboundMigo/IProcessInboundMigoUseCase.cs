namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

public interface IProcessInboundMigoUseCase
{
    string TransactionType { get; }
    Task<UseCaseOutput> ExecuteAsync(string transactionId, CancellationToken cancellationToken);
}

public interface IProcessInboundMigoUseCase<TInput, TOutput>
    : IProcessInboundMigoUseCase
    where TInput : UseCaseInput
    where TOutput : UseCaseOutput
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
