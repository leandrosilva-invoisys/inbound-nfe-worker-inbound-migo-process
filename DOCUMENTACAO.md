# Documentação – Worker Template (.NET 8)

Documentação resumida do template de worker para o time de desenvolvimento.

---

## Visão geral

Template de **worker .NET 8** em camadas, pronto para ser clonado e adaptado. Inclui dois modos de execução:

- **Worker em loop** – job que roda periodicamente (ex.: a cada X minutos), busca itens no repositório e processa.
- **Consumer SQS** – consome mensagens de uma fila (invoisys.SDK.Queue) e processa cada mensagem.

---

## O que está implementado

### Estrutura de projetos

| Projeto | Responsabilidade |
|--------|-------------------|
| **HelloWorld.Application** | Interfaces, DTOs e casos de uso (regra de negócio). |
| **HelloWorld.Infra** | Implementações: repositório (dados) e API client (HTTP). |
| **HelloWorld.Infra.Worker** | Host, `BackgroundService` (Worker), Consumer SQS, serviços de processamento, logging e telemetria. |
| **HelloWorld.Test** | Testes unitários. |

### Logging (Serilog)

- **CorrelationId** e **TraceId** em todos os logs (enrichers: `CorrelationIdEnricher` do Traceability e `TraceIdEnricher` local).
- Template de log no console: `CorrelationId`, `TraceId` e mensagem.
- Uso de `LogContext.PushProperty("LogId", ...)` para correlacionar logs por execução/mensagem.

### Telemetria (OpenTelemetry)

- **Tracing**: ActivitySource (`HelloWorldActivitySource`), instrumentação de HttpClient, export para Console e OTLP (em não-Development).
- **Metrics**: instrumentação de HttpClient, export para Console e OTLP (em não-Development).
- **Traceability**: `AddTraceability(serviceName)` para correlation-id e integração com o ecossistema.

### Worker (job em loop)

- `Worker.cs` – `BackgroundService` que em loop:
  - Cria escopo por iteração.
  - Obtém IDs para processar via `IHelloWorldRepository.ObterIdsParaProcessarAsync()`.
  - Para cada ID chama o use case `IProcessHelloWorldUseCase.Execute(...)`.
  - Aguarda intervalo configurável (ex.: `Task.Delay(TimeSpan.FromMinutes(1))`).
- Respeita `CancellationToken` para desligamento graceful.

### Consumer SQS

- **Consumer.cs** – implementa `IConsumer<QueueMessageDTO>` com `[QueueName("0000-invs-inbound-hello-world")]`.
- **HelloWorldProcessService** – deserializa o body da mensagem (JSON com `Id`), chama o mesmo use case `ProcessHelloWorld` e retorna `true`/`false` para sucesso/falha (controle de remoção/retry na fila).
- **QueueMessageDTO** – DTO com `Body` (string); payload interno esperado: `{ "Id": number }`.

### Caso de uso

- **ProcessHelloWorldUseCase** – orquestra: repositório (`ObterDadoPorIdAsync`) → API client (`EnviarAsync`) e retorna `ProcessHelloWorldOutput` (Sucesso, Mensagem).

### Infra

- **HelloWorldRepository** – implementa `IHelloWorldRepository` (obter IDs para processar e dado por ID); adaptar para seu banco/armazenamento.
- **HelloWorldApiClient** – implementa `IHelloWorldApiClient` (HTTP); `BaseAddress` em `appsettings` (chave `HelloWorld:BaseAddress`).

### SDKs e configuração

- **invoisys.SDK.Host** – construção do host.
- **invoisys.SDK.Queue** – fila SQS (`AddSQSQueue()`); consumer registrado via atributo `[QueueName]`.
- **invoisys.SDK.SecretManager** e **invoisys.SDK.Storage** – registrados no `Program.cs` (ajustar conforme uso).

### CI/CD

- **.github/workflows/ci.yml** – build e testes em PRs (branches dev, qa, sbx, prd).
- **.github/workflows/deploy.yml** – build, testes, build Docker e deploy ECS; variáveis de ambiente resolvidas por branch; TODOs para `deployment_name`, `project_name`, `dockerfile_path`, `ecr_repo`.

### Docker

- **Dockerfile** no projeto `HelloWorld.Infra.Worker` – build e execução do worker em container.

---

## Como executar

```bash
cd src/HelloWorld.Infra.Worker
dotnet run
```

Build e testes:

```bash
cd src
dotnet build HelloWorldWorker.sln
dotnet test HelloWorld.Test/HelloWorld.Test.csproj
```

Docker (a partir da pasta `src`):

```bash
docker build -f HelloWorld.Infra.Worker/Dockerfile -t hello-world-worker .
docker run --rm hello-world-worker
```

---

## Configuração mínima (appsettings)

- **HelloWorld:BaseAddress** – URL base do API client (ex.: `https://api.example.com`).
- **OpenTelemetry:OtlpEndpoint** – (opcional) endpoint OTLP para tracing/metrics em não-Development.
- **OpenTelemetry:EnableMetricsConsoleExporter** – (opcional) default `true` para export de métricas no console.

---

## O que customizar ao criar um novo worker

1. **Substituir “HelloWorld”** por o nome do domínio (projetos, namespaces, classes, config, workflows).
2. **Worker**: intervalo do loop, origem dos IDs e uso do use case conforme sua regra.
3. **Consumer**: nome da fila em `[QueueName("...")]`, DTO da mensagem e payload (ex.: além de `Id`).
4. **Use case**: lógica de negócio, repositório e clientes (API, storage, etc.).
5. **Repositório/API client**: implementação real (banco, APIs).
6. **UserSecretsId** no `.csproj` do Worker – trocar `REPLACE-WITH-YOUR-GUID` por um GUID único.
7. **Deploy**: em `deploy.yml`, ajustar `deployment_name`, `project_name`, `dockerfile_path`, `ecr_repo` e variáveis do repositório.

---

*Documento resumido para o time de desenvolvimento. Detalhes de uso do template estão no [README.md](README.md).*
