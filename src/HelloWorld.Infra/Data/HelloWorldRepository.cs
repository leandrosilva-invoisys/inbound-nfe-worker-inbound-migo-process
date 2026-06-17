// Implementação de IHelloWorldRepository. Substitua pela sua fonte de dados (SQL, PostgreSQL, etc.).

using HelloWorld.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HelloWorld.Infra.Data;

public class HelloWorldRepository : IHelloWorldRepository
{
    private readonly IConfiguration _configuration;

    public HelloWorldRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<int>> ObterIdsParaProcessarAsync()
    {
        await Task.CompletedTask;
        // TODO: Substituir pela consulta real (ex: Dapper + SqlConnection, Npgsql).
        // Exemplo: SELECT id FROM dbo.Tabela WHERE status = 1 AND ...
        return new List<int>();
    }

    public async Task<string> ObterDadoPorIdAsync(int id)
    {
        await Task.CompletedTask;
        // TODO: Substituir pela consulta real que retorna o dado (ex: payload, nome arquivo).
        return string.Empty;
    }
}
