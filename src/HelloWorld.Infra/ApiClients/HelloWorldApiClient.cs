// Cliente HTTP para API externa. Substitua pela sua API (auth, endpoints, retry com Polly).

using System.Text;
using System.Text.Json;
using HelloWorld.Application.DTO;
using HelloWorld.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelloWorld.Infra.ApiClients;

public class HelloWorldApiClient : IHelloWorldApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HelloWorldApiClient> _logger;
    private readonly IConfiguration _configuration;

    public HelloWorldApiClient(
        HttpClient httpClient,
        ILogger<HelloWorldApiClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<ResponseHelloWorld?> EnviarAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Ajustar endpoint e headers (ex: Authorization Bearer, OAuth).
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hello")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<ResponseHelloWorld>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar API HelloWorld");
            return null;
        }
    }
}
