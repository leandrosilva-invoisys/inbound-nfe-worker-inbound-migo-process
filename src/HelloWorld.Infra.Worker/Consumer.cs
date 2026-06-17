using HelloWorld.Infra.Worker.Model;
using HelloWorld.Infra.Worker.Services;
using invoisys.SDK.Queue.Attributes;
using invoisys.SDK.Queue.Interfaces;

namespace HelloWorld.Infra.Worker;

/// <summary>
/// Template de consumer para leitura de mensagens de uma fila (SQS, etc.).
/// Use este arquivo como referência quando for necessário consumir mensagens de uma fila:
/// processamento assíncrono, workers que reagem a eventos enfileirados, integrações via fila, etc.
/// </summary>
/// <remarks>
/// <para><b>Quando usar:</b> sempre que o worker precisar ler mensagens de uma fila (ex.: SQS, RabbitMQ).</para>
/// <para><b>Como adaptar:</b></para>
/// <list type="bullet">
///   <item>Altere o atributo <c>[QueueName("...")]</c> para o nome da fila desejada.</item>
///   <item>Substitua <c>QueueMessageDTO</c> pelo DTO que representa a mensagem da sua fila.</item>
///   <item>Implemente um serviço de processamento (ex.: IHelloWorldProcessService) e injete no construtor.</item>
///   <item>O retorno de <c>Consume</c>: <c>true</c> = mensagem processada com sucesso (pode ser removida da fila); <c>false</c> = falha (reprocessamento/retry conforme política da fila).</item>
/// </list>
/// </remarks>
[QueueName("0000-invs-inbound-hello-world")]
public class Consumer : IConsumer<QueueMessageDTO>
{
    private readonly IHelloWorldProcessService _helloWorldProcessService;

    public Consumer(IHelloWorldProcessService helloWorldProcessService)
    {
        _helloWorldProcessService = helloWorldProcessService;
    }

    /// <summary>
    /// Processa uma mensagem recebida da fila. Retorna true para sucesso (mensagem pode ser removida) ou false para falha (retry conforme política da fila).
    /// </summary>
    public async Task<bool> Consume(QueueMessageDTO messageDto)
    {
        var resultado = await _helloWorldProcessService.Handle(messageDto);
        return resultado;
    }
}
