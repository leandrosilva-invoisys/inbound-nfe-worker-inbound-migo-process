// Interface para chamadas a API externa. Substitua HelloWorld e ResponseHelloWorld pelo seu domínio.

using HelloWorld.Application.DTO;

namespace HelloWorld.Application.Interfaces;

public interface IHelloWorldApiClient
{
    /// <summary>
    /// Envia dados para API externa. Ajuste payload e retorno conforme seu contrato (ex: EnvioAsync, NotificarAsync).
    /// </summary>
    Task<ResponseHelloWorld?> EnviarAsync(string payload, CancellationToken cancellationToken);
}
