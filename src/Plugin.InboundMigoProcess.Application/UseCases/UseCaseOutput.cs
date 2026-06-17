namespace Plugin.InboundMigoProcess.Application.UseCases;

public abstract class UseCaseOutput
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
