# ADR 002: Estratégia de Sincronização (Offline-First)

## Status
Accepted (Outbox Pattern validado via PoC)

## Contexto
Clínicas veterinárias frequentemente enfrentam instabilidade de internet e precisam continuar operando (Offline-First) de forma transparente, com sincronização automática quando a conexão é restabelecida.

## Decisão
Após a execução de Provas de Conceito (PoCs) na suíte `PoC.SyncTests` e validação com o projeto `PoC.Sync`, decidimos adotar o **Outbox Pattern (Transactional Outbox)** implementado manualmente na aplicação (Background Worker no cliente pushing requisições pendentes para a API Central).

A abordagem via `Dotmim.Sync` foi descontinuada devido à exigência de alterar schemas no SQL Server com tabelas de tracking e incompatibilidade com SQLite InMemory/arquivos SQLite locais como nó servidor para testes TDD.

Documentação detalhada e diagrama final de sequência foram gerados (veja `docs/diagramas/sync-sequence.mmd`).

## Consequências
- Implementamos `OutboxMessage` no `OfflineDbContext`.
- Worker/HostedService no MAUI/Blazor enviará requisições HTTP seguras com retry exponencial, validado com sucesso na PoC de fila.
- Endpoints de sync no Backend tratarão idempotência, resolução de conflito (LWW / RowVersion) e sincronização reversa (Pull por timestamp).

