# Inbound NFe Worker - Inbound MIGO Process

Worker .NET 8 para processamento inbound de MIGO via fila SQS, com roteamento por `transactionType`.

## Estrutura inicial

O projeto foi preparado com:

- arquitetura em camadas (`Application`, `Infra`, `Infra.Worker`, `Test`);
- consumer SQS com payload por `TransactionId`;
- factory de use cases por `transactionType`;
- dois use cases stub (sem regra de negócio):
- `inbound.sync.migo.reverse`;
  - `inbound.sync.inbound-delivery.confirm`.

## Build e testes

```bash
cd src
dotnet build Plugin.InboundMigoProcess.sln
dotnet test Plugin.InboundMigoProcess.Test/Plugin.InboundMigoProcess.Test.csproj
```

## Docker

```bash
cd src
docker build -f Plugin.InboundMigoProcess.Infra.Worker/Dockerfile -t inbound-migo-worker .
docker run --rm inbound-migo-worker
```
