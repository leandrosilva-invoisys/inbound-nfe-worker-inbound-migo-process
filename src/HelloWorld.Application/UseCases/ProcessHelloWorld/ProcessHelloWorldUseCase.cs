// Implementação do caso de uso. Orquestra repositório, storage (se usar) e API client.
// Substitua HelloWorld pelo nome do seu fluxo e adapte a lógica de negócio.

using HelloWorld.Application.Interfaces;
using Serilog;
using Serilog.Context;

namespace HelloWorld.Application.UseCases.ProcessHelloWorld;

public class ProcessHelloWorldUseCase : IProcessHelloWorldUseCase
{
    private readonly IHelloWorldRepository _helloWorldRepository;
    private readonly IHelloWorldApiClient _helloWorldApiClient;

    public ProcessHelloWorldUseCase(
        IHelloWorldRepository helloWorldRepository,
        IHelloWorldApiClient helloWorldApiClient)
    {
        _helloWorldRepository = helloWorldRepository;
        _helloWorldApiClient = helloWorldApiClient;
    }

    public async Task<ProcessHelloWorldOutput> Execute(ProcessHelloWorldInput input, Guid logId, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("LogId", logId))
        using (LogContext.PushProperty("Id", input.Id))
        {
            try
            {
                Log.Debug("Iniciando processamento HelloWorld");

                var dado = await _helloWorldRepository.ObterDadoPorIdAsync(input.Id);
                if (string.IsNullOrEmpty(dado))
                {
                    Log.Error("Dado não encontrado para o Id informado");
                    return new ProcessHelloWorldOutput { Sucesso = false, Mensagem = "Dado não encontrado" };
                }

                var response = await _helloWorldApiClient.EnviarAsync(dado, cancellationToken);
                if (response == null)
                {
                    Log.Error("Resposta da API é nula");
                    return new ProcessHelloWorldOutput { Sucesso = false, Mensagem = "Resposta nula" };
                }

                Log.Information("Processamento concluído. Sucesso: {Sucesso}", response.Sucesso);
                return new ProcessHelloWorldOutput
                {
                    Sucesso = response.Sucesso,
                    Mensagem = response.Mensagem ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao processar HelloWorld");
                return new ProcessHelloWorldOutput { Sucesso = false, Mensagem = ex.Message };
            }
        }
    }
}
