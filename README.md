# Template Worker .NET (Boilerplate)

Template genérico para criação de **workers .NET 8** em camadas (Application, Infra, Infra.Worker, Test). Use como repositório modelo no GitHub para gerar novos projetos.

## Como usar este template

1. **Crie um novo repositório** a partir deste template (GitHub: *Use this template*).
2. **Substitua todas as ocorrências de `HelloWorld`** pelo nome do seu domínio/projeto (ex: `NFeEpec`, `Pedido`, `Notificacao`).
3. Ajuste namespaces, nomes de classes, interfaces e projetos conforme a tabela abaixo.

### O que substituir

| Placeholder / Nome no template | Substitua por (exemplo) |
|--------------------------------|-------------------------|
| **HelloWorld** (namespace, projeto, classe) | Nome do seu domínio (ex: `NFeEpec`, `Pedido`) |
| **HelloWorldWorker.sln** | `SeuProjetoWorker.sln` |
| **HelloWorld.Application** | `SeuProjeto.Application` |
| **HelloWorld.Infra** | `SeuProjeto.Infra` |
| **HelloWorld.Infra.Worker** | `SeuProjeto.Infra.Worker` |
| **HelloWorld.Test** | `SeuProjeto.Test` |
| **IHelloWorldRepository** | Sua interface de dados (ex: `INFeGateway`, `IPedidoRepository`) |
| **IHelloWorldApiClient** | Sua interface de API externa (ex: `INFeSefazGateway`) |
| **ProcessHelloWorld** (use case) | Nome do fluxo (ex: `ProcessarEpec`, `ProcessarPedido`) |
| **ResponseHelloWorld** | DTO de resposta da sua API |
| **HelloWorld:BaseAddress** (config) | Chave de configuração da sua API |

### Arquivos que precisam de alteração após substituir o nome

- Todos os `.csproj` e `.sln` (nomes de projeto e referências).
- Todos os `.cs` (namespaces e nomes de tipos).
- `Program.cs`, `Worker.cs`, `appsettings.json`, `launchSettings.json`.
- `Dockerfile` (COPY e ENTRYPOINT).
- `.github/workflows/*.yml` (project_name, deployment_name, dockerfile_path, etc.).
- `UserSecretsId` no `.csproj` do Worker (substitua `REPLACE-WITH-YOUR-GUID` por um GUID único).

## Estrutura do projeto

```
src/
├── HelloWorld.Application/        # Casos de uso, DTOs, interfaces
│   ├── DTO/
│   ├── Interfaces/
│   └── UseCases/ProcessHelloWorld/
├── HelloWorld.Infra/              # Implementações (repositório, API client)
│   ├── ApiClients/
│   └── Data/
├── HelloWorld.Infra.Worker/       # Host e BackgroundService
│   ├── Program.cs
│   ├── Worker.cs
│   ├── appsettings.json
│   └── Dockerfile
└── HelloWorld.Test/                # Testes unitários
```

## Tecnologias

- **.NET 8**
- **Serilog** (logging)
- **invoisys.SDK.Host**, **Storage**, **SecretManager**, **Queue** (quando necessário)
- **Dapper**, **Polly**, **Microsoft.Data.SqlClient**, **Npgsql** (na Infra, conforme necessidade)
- **NUnit**, **Moq** (testes)

## Executar localmente

```bash
cd src/HelloWorld.Infra.Worker
dotnet run
```

## Build e testes

```bash
cd src
dotnet build HelloWorldWorker.sln
dotnet test HelloWorld.Test/HelloWorld.Test.csproj
```

## Docker

```bash
cd src
docker build -f HelloWorld.Infra.Worker/Dockerfile -t hello-world-worker .
docker run --rm hello-world-worker
```

## CI/CD

- **.github/workflows/ci.yml** – Build e testes em PRs (ajuste `working_directory` e nomes).
- **.github/workflows/deploy.yml** – Deploy (ajuste `deployment_name`, `project_name`, `dockerfile_path`, `ecr_repo`).

Substitua `HelloWorld` em todo o repositório antes de configurar os workflows para seu projeto.
