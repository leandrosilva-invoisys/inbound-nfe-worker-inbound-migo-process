# Documentacao - Worker Inbound MIGO

## Visao geral

Projeto inicial do worker `inbound-nfe-worker-inbound-migo-process`, baseado no padrao do worker de inbound delivery.

Fluxo principal:

1. Consumer recebe mensagem com `TransactionId`.
2. Service busca `transactionType` no repositorio.
3. Factory resolve o use case pelo `transactionType`.
4. Use case stub executa e retorna sucesso.

## Transaction types mapeados

- `inbound.sync.migo.reverse` (cancelamento)
- `inbound.sync.inbound-delivery.confirm` (envio)

## Pontos importantes

- Estrutura inicial criada sem logica de negocio dos use cases.
- Repositorio de transacao em modo stub para permitir bootstrap.
- Pipeline CI e deploy ajustados para o contexto do worker MIGO.
