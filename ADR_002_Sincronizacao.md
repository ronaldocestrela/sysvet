# Architecture Decision Record (ADR-002)
## Tema: Estratégia de Sincronização de Dados (Offline-First)

**Data:** 18/08/2026
**Status:** Decidido

### 1. Contexto e Problema
O VetNexus está implementando uma estratégia *Offline-First* nos clientes MAUI (App Nativo) e PWA (WebAssembly). Precisávamos definir um motor de sincronização de dados bidirecional que levasse as alterações do SQLite local para o SQL Server central. Duas estratégias foram cogitadas:
- O uso de um framework como o `Dotmim.Sync`.
- A implementação manual orientada a eventos utilizando o `Outbox Pattern`.

Para fundamentar essa decisão arquitetural, executamos testes de Prova de Conceito (PoC) implementados na suíte de testes `PoC.SyncTests`.

### 2. Resultados das Provas de Conceito (PoCs)

#### Abordagem A: Dotmim.Sync
* **Vantagens:** Sincronização delta nativa e altamente automatizada, tratamento inteligente de conflitos por *timestamp*.
* **Desvantagens/Bloqueios:** A arquitetura do Dotmim exige explicitamente que o nó servidor (*ServerProvider*) seja um banco de dados real suportado (SQL Server, Postgres, MySql, etc). **Ele não suporta SQLite InMemory ou arquivos SQLite comuns atuando como servidor central.** Adicionalmente, para habilitar a sincronização, ele altera o schema do banco SQL injetando diversas tabelas de *tracking* (`_tracking`, `_scope`), poluindo os domínios do EF Core e quebrando o princípio de persistência limpa sem impacto. A dependência excessiva em um motor SQL específico dificulta testes de integração TDD contínuos que geralmente rodam em instâncias InMemory.

#### Abordagem B: Outbox Pattern (Transactional Outbox)
* **Vantagens:** Controle total do tráfego. Como vimos no teste `Worker_Should_Process_OutboxMessage_And_Sync_To_CentralDb`, salvar uma entidade e o seu evento (Ex: `TutorCreated`) numa mesma transação `SaveChangesAsync()` foi trivial e 100% suportado pelo EF Core. É perfeitamente agnóstico em relação ao banco de dados, permitindo que a integração ocorra em InMemory (TDD), SQLite (Local) ou SQL Server (Produção).
* **Desvantagens:** Requer escrita de rotinas manuais (Workers/BackgroundServices) e controle de retentativas na chamada para o servidor, o que gera maior overhead de programação (apesar de a lógica ser simples).

### 3. Decisão
Optamos por utilizar o **Outbox Pattern**.

A limitação técnica imposta pelo *Dotmim.Sync* na etapa de testes e a severa violação da *Clean Architecture* na persistência central (injeção forçada de tabelas de metadados no SQL Server) o tornaram incompatível com nossos princípios de design de domínio e TDD.
O **Outbox Pattern** será implementado no `Clients.Infrastructure` interceptando o `SaveChangesAsync()` do EF Core (como feito na PoC) para salvar eventos na tabela `OutboxMessages`. Um `BackgroundService` agendado no MAUI e no Blazor fará o papel de consumir essa fila localmente e fazer as requisições `HTTP/REST` seguras para o Backend (API) consolidar as mudanças no DB Central.

### 4. Consequências e Próximos Passos
* Adicionaremos uma entidade genérica `OutboxMessage` no `OfflineDbContext`.
* Criaremos um Worker ou HostedService no app cliente para enviar as requisições pendentes de maneira assíncrona.
* O Backend (API) precisará expor *Endpoints* de sincronização (ex: `POST /api/sync/push`) preparados para tratar a idempotência dessas chamadas.
