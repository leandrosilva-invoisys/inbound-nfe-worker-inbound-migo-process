namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;

public sealed class InboundMigoUseCaseFactory : IInboundMigoUseCaseFactory
{
    private readonly IReadOnlyDictionary<string, IProcessInboundMigoUseCase> _useCases;

    public InboundMigoUseCaseFactory(IEnumerable<IProcessInboundMigoUseCase> useCases)
    {
        _useCases = useCases.ToDictionary(x => x.TransactionType, StringComparer.OrdinalIgnoreCase);
    }

    public IProcessInboundMigoUseCase? Resolve(string? transactionType)
    {
        if (string.IsNullOrWhiteSpace(transactionType))
            return null;

        return _useCases.TryGetValue(transactionType, out var useCase)
            ? useCase
            : null;
    }
}
