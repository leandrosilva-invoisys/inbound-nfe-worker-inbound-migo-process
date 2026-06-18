using System.Text.Json.Serialization;

namespace Plugin.InboundMigoProcess.Application.DTO;

public sealed class TagManagerTagDto
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("idTag")]
    public long TagId { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("tagValues")]
    public string TagValue { get; init; } = string.Empty;

    [JsonPropertyName("tagProperties")]
    public string TagProperties { get; init; } = string.Empty;
}

public sealed class TagManagerRequestUpdate
{
    public required long SubscriptionId { get; init; }
    public required long Id { get; init; }
    public int? Status { get; init; }
    public string? TagValue { get; init; }
}

public sealed class TagManagerRequestAdd
{
    public required long SubscriptionId { get; init; }
    public required long DocId { get; init; }
    public required int Status { get; init; }
    public required string TagValue { get; init; }
    public required string TaggedBy { get; init; }
    public required string TagName { get; init; }
    public bool? Substituir { get; init; }
}

public sealed class TagManagerRequestRemove
{
    public required long SubscriptionId { get; init; }
    public required long Id { get; init; }
}
