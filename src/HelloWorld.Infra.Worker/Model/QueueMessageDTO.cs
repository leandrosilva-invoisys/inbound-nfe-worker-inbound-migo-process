// DTO da mensagem da fila. Alinhe com o contrato do invoisys.SDK.Queue se necessário (ex.: nome do tipo no SDK).

namespace HelloWorld.Infra.Worker.Model;

public class QueueMessageDTO
{
    public string Body { get; set; } = string.Empty;
}
