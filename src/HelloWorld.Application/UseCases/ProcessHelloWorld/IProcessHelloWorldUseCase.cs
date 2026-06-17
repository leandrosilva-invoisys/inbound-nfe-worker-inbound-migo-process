// Caso de uso principal do worker. Substitua ProcessHelloWorld pelo nome do seu fluxo (ex: ProcessarEpec, ProcessarPedido).

namespace HelloWorld.Application.UseCases.ProcessHelloWorld;

public interface IProcessHelloWorldUseCase
{
    Task<ProcessHelloWorldOutput> Execute(ProcessHelloWorldInput input, Guid logId, CancellationToken cancellationToken);
}
