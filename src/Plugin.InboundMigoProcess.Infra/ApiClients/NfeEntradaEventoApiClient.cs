using System.Net.Http.Json;
using System.Text.Json;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Plugin.InboundMigoProcess.Infra.ApiClients;

public sealed class NfeEntradaEventoApiClient : INfeEntradaEventoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NfeEntradaEventoApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NfeEntradaEventoApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NfeEntradaEventoApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> AdicionarEventoVinculoAsync(int nfeEntradaId, string descricao, CancellationToken cancellationToken)
    {
        var token = _configuration["NfeEntradaApi:AdicionarEventoVinculoToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError("NfeEntradaApi:AdicionarEventoVinculoToken não configurado.");
            return false;
        }

        var path = $"api/nfeentrada/adicionareventovinculo/{Uri.EscapeDataString(token)}";
        var body = new NfeEntradaEventoVinculoRequestDto { Id = nfeEntradaId, Descricao = descricao };

        using var response = await _httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
            return true;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning(
            "Falha ao registrar evento de vínculo NFe entrada: status={Status} body={Body} nfeEntradaId={Id}",
            (int)response.StatusCode,
            responseBody,
            nfeEntradaId);
        return false;
    }
}
