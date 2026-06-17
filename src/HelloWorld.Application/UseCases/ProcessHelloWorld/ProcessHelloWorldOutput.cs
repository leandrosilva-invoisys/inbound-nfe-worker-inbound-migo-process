// Resultado do processamento. Ajuste propriedades conforme necessidade (ex: StatusSefaz, TentarNovamenteMaisTarde).

namespace HelloWorld.Application.UseCases.ProcessHelloWorld;

public class ProcessHelloWorldOutput
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
