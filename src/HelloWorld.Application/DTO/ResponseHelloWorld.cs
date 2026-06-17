// DTOs de resposta da API externa. Substitua pelos DTOs reais do seu domínio.

namespace HelloWorld.Application.DTO;

public class ResponseHelloWorld
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public string? Codigo { get; set; }
}
