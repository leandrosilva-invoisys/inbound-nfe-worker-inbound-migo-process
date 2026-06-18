using System.Text.Json;
using System.Text.Json.Nodes;
using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Services;

public interface ITagValueBuilder
{
    string BuildInboundMigoCancelamentoTagValue(
        string transactionId,
        string materialDocument,
        string matDocumentYear,
        string resultado,
        List<SapReturnMessageDto> messages);

    string BuildInboundMigoTagValueLimpo(string? currentTagValue, bool removerCamposMovimentacao);
}

public sealed class TagValueBuilder : ITagValueBuilder
{
    public string BuildInboundMigoCancelamentoTagValue(
        string transactionId,
        string materialDocument,
        string matDocumentYear,
        string resultado,
        List<SapReturnMessageDto> messages)
    {
        var payload = new
        {
            transactionId,
            materialdocument = materialDocument,
            matdocumentyear = matDocumentYear,
            resultado,
            messages = messages.Select(m => new
            {
                m.Type,
                m.Id,
                m.Number,
                m.Message,
                m.LogNo,
                m.LogMsgNo,
                m.MessageV1,
                m.MessageV2,
                m.MessageV3,
                m.MessageV4
            })
        };

        return JsonSerializer.Serialize(payload);
    }

    public string BuildInboundMigoTagValueLimpo(string? currentTagValue, bool removerCamposMovimentacao)
    {
        if (string.IsNullOrWhiteSpace(currentTagValue))
            return JsonSerializer.Serialize(new { });

        JsonObject obj;
        try
        {
            obj = JsonNode.Parse(currentTagValue) as JsonObject ?? new JsonObject();
        }
        catch (JsonException)
        {
            obj = new JsonObject();
        }

        if (removerCamposMovimentacao)
        {
            obj.Remove("materialdocument");
            obj.Remove("matdocumentyear");
            obj.Remove("MATERIALDOCUMENT");
            obj.Remove("MATDOCUMENTYEAR");
        }

        return obj.ToJsonString();
    }
}
