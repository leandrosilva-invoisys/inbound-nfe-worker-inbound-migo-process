using System.Text.Json.Serialization;

namespace Plugin.InboundMigoProcess.Infra.Worker.Model;

public class QueueMessageDTO
{
    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("subscription")]
    public string? Subscription { get; set; }
}
