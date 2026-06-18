using System.Text.Json;
using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Services;

public interface ITransactionResultMapper
{
    InboundTransactionResultDto Map(string resultRaw);
}

public sealed class TransactionResultMapper : ITransactionResultMapper
{
    public InboundTransactionResultDto Map(string resultRaw)
    {
        if (string.IsNullOrWhiteSpace(resultRaw))
            return new InboundTransactionResultDto();

        try
        {
            using var doc = JsonDocument.Parse(resultRaw);
            return ParseRoot(doc.RootElement);
        }
        catch (JsonException)
        {
            return new InboundTransactionResultDto();
        }
    }

    private static InboundTransactionResultDto ParseRoot(JsonElement root)
    {
        var mapped = ParseMessages(root);
        if (mapped.Return.Count > 0)
            return mapped;

        if (root.TryGetProperty("transactionData", out var transactionData) &&
            transactionData.ValueKind == JsonValueKind.Object)
            return ParseMessages(transactionData);

        return mapped;
    }

    private static InboundTransactionResultDto ParseMessages(JsonElement root)
    {
        var messages = new List<SapReturnMessageDto>();

        if (root.TryGetProperty("RETURN", out var returnElement) && returnElement.ValueKind == JsonValueKind.Array)
            messages.AddRange(ParseArray(returnElement));

        if (messages.Count == 0 && root.TryGetProperty("MESSAGES", out var messagesElement) && messagesElement.ValueKind == JsonValueKind.Array)
            messages.AddRange(ParseArray(messagesElement));

        return new InboundTransactionResultDto { Return = messages };
    }

    private static IEnumerable<SapReturnMessageDto> ParseArray(JsonElement array)
    {
        foreach (var item in array.EnumerateArray())
        {
            yield return new SapReturnMessageDto
            {
                Type = GetString(item, "TYPE"),
                Id = GetString(item, "ID"),
                Number = GetString(item, "NUMBER"),
                Message = GetString(item, "MESSAGE"),
                LogNo = GetString(item, "LOGNO"),
                LogMsgNo = GetString(item, "LOGMSGNO"),
                MessageV1 = GetString(item, "MESSAGEV1"),
                MessageV2 = GetString(item, "MESSAGEV2"),
                MessageV3 = GetString(item, "MESSAGEV3"),
                MessageV4 = GetString(item, "MESSAGEV4")
            };
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return string.Empty;

        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.GetRawText();
    }
}
