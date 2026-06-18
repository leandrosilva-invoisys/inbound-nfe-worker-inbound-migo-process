namespace Plugin.InboundMigoProcess.Application.DTO;

public sealed class InboundTransactionRecordDto
{
    public Guid TransactionId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public long SubscriptionId { get; init; }
    public long? DocId { get; init; }
    public string OriginatorService { get; init; } = string.Empty;
    public long? TagId { get; init; }
    public string ResultRaw { get; init; } = string.Empty;
}
