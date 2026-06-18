using invoisys.SDK.SecretManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Plugin.InboundMigoProcess.Infra.Data;

internal static class PostgreSqlCd30ConnectionResolver
{
    internal static Task<string> ResolveAsync(IConfiguration configuration, ISecretService secretService)
    {
        var fromConfig = configuration["ConnectionStrings:PostgreSqlCd30"];
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return Task.FromResult(fromConfig);

        return ResolveFromSecretAsync(secretService);
    }

    private static async Task<string> ResolveFromSecretAsync(ISecretService secretService)
    {
        var secret = await secretService.GetSecretString("connectionStringPostgresqlCd30").ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "Connection string PostgreSQL CD30 não encontrada: defina ConnectionStrings:PostgreSqlCd30 (appsettings local/UserSecrets) " +
                "ou configure o segredo AWS connectionStringPostgresqlCd30.");
        }

        return secret;
    }
}
