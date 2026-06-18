using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Plugin.InboundMigoProcess.Infra.ApiClients;

public sealed class TagManagerApiClient : ITagManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TagManagerApiClient> _logger;

    public TagManagerApiClient(HttpClient httpClient, ILogger<TagManagerApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagManagerTagDto?> ObterTagAsync(long subscriptionId, long docId, string tagName, CancellationToken cancellationToken)
    {
        var url = $"api/plt/tagmanager/inbound/nfe/list?SubscriptionId={subscriptionId}&DocId={docId}&TagName={Uri.EscapeDataString(tagName)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Erro ao consultar TagManager. status={StatusCode} body={Body}", (int)response.StatusCode, body);
            return null;
        }

        var tags = await response.Content.ReadFromJsonAsync<List<TagManagerTagDto>>(cancellationToken: cancellationToken);
        return tags?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<TagManagerTagDto>> ObterTagsAsync(long subscriptionId, long docId, string tagName, CancellationToken cancellationToken)
    {
        var url = $"api/plt/tagmanager/inbound/nfe/list?SubscriptionId={subscriptionId}&DocId={docId}&TagName={Uri.EscapeDataString(tagName)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return Array.Empty<TagManagerTagDto>();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Erro ao listar TagManager. status={StatusCode} body={Body}", (int)response.StatusCode, body);
            return Array.Empty<TagManagerTagDto>();
        }

        var tags = await response.Content.ReadFromJsonAsync<List<TagManagerTagDto>>(cancellationToken: cancellationToken);
        return (IReadOnlyList<TagManagerTagDto>?)tags ?? Array.Empty<TagManagerTagDto>();
    }

    public async Task<bool> AtualizarTagAsync(TagManagerRequestUpdate request, CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Patch, "api/plt/tagmanager/inbound/nfe/update")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _httpClient.SendAsync(message, cancellationToken);
        if (response.IsSuccessStatusCode)
            return true;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Erro ao atualizar tag {TagId}. status={StatusCode} body={Body}", request.Id, (int)response.StatusCode, body);
        return false;
    }

    public async Task<long?> CriarTagAsync(TagManagerRequestAdd request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/plt/tagmanager/inbound/nfe/add", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Erro ao criar tag {TagName}. docId={DocId} status={StatusCode} body={Body}",
                request.TagName, request.DocId, (int)response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TagCreateResponseDto>(cancellationToken: cancellationToken);
        return result?.Id;
    }

    public async Task<bool> RemoverTagAsync(TagManagerRequestRemove request, CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Delete, "api/plt/tagmanager/inbound/nfe/remove")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _httpClient.SendAsync(message, cancellationToken);
        if (response.IsSuccessStatusCode)
            return true;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Erro ao remover tag {TagId}. status={StatusCode} body={Body}", request.Id, (int)response.StatusCode, body);
        return false;
    }
}

internal sealed class TagCreateResponseDto
{
    [JsonPropertyName("id")]
    public long Id { get; init; }
}
