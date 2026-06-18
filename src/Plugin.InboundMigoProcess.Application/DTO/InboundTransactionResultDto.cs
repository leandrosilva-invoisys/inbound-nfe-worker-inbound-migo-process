namespace Plugin.InboundMigoProcess.Application.DTO;

public sealed class InboundTransactionResultDto
{
    public List<SapReturnMessageDto> Return { get; init; } = [];
}

public sealed class SapReturnMessageDto
{
    public string Type { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string LogNo { get; init; } = string.Empty;
    public string LogMsgNo { get; init; } = string.Empty;
    public string MessageV1 { get; init; } = string.Empty;
    public string MessageV2 { get; init; } = string.Empty;
    public string MessageV3 { get; init; } = string.Empty;
    public string MessageV4 { get; init; } = string.Empty;
}
