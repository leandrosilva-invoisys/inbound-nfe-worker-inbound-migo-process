// Serviço que processa mensagens da fila. Retorna true para sucesso (remover da fila) e false para reprocessar.

using HelloWorld.Infra.Worker.Model;

namespace HelloWorld.Infra.Worker.Services;

public interface IHelloWorldProcessService
{
    Task<bool> Handle(QueueMessageDTO messageDto);
}
