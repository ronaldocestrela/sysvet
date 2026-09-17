# ADR 014: SQLite local nos clients (CRM offline)

## Status
Accepted

## Data
2026-09-16

## Contexto

A Fase 3.4 exige CRUD tutor/pet **sem rede** em Blazor WASM (PWA) e MAUI, alinhado a [`agents.md`](../agents.md) (EF Core 10 + SQLite local) e ao Outbox da [ADR-002](./ADR-002-estrategia-de-sync.md). As telas CRM já existiam via API ([ADR-012](./ADR-012-blazor-pwa-jwt.md), [ADR-013](./ADR-013-maui-blazor-hybrid.md)); faltava schema local, migrations isoladas e adapter com o mesmo contrato DTO da nuvem.

No WASM, `UseSqlite("Data Source=sysvet.db")` usa MEMFS e perde dados no refresh — era necessário persistir o arquivo entre sessões.

## Opções consideradas

1. **EF Core SQLite + migrations próprias** — espelha entidades `Tutor`/`Pet` de `Core.Domain`; outbox tutor na mesma transação.
   - Prós: alinhado ao stack; reutiliza portas `ITutorRepository`/`IPetRepository`; testável com `:memory:`.
   - Contras: schema local distinto da nuvem (sem Identity/AuditLog).

2. **Repositório leve (SQL manual / Dapper)** — roadmap listava como alternativa.
   - Prós: menor cold start no WASM.
   - Contras: duplica mapeamento de VOs e soft delete; diverge de ADR-002 (EF + outbox).

3. **SqliteWasmBlazor (OPFS)** — pacote de terceiro com EF completo no browser.
   - Prós: persistência nativa OPFS.
   - Contras: dependência pre-release; fora do padrão MAUI/WASM compartilhado escolhido pelo time.

4. **Persistência WASM:** snapshot **IndexedDB** do arquivo `.db` vs OPFS vs MAUI-only.
   - Escolhido: **IndexedDB** (restore no boot, flush após `SaveChanges`) — sem pacote extra; MAUI usa arquivo em `AppDataDirectory` (no-op).

## Decisão

Adotar **EF Core SQLite** em `Clients.Infrastructure` com **`OfflineDbContext`**, migration **`InitialOffline`** independente do `CoreDbContext`, repositórios **`OfflineTutorRepository`/`OfflinePetRepository`**, e stores **`ITutorStore`/`IPetStore`** com implementações **`Offline*Store`** (ativo) e **`Http*Store`** (contrato API para sync 3.5).

SharedUI (`Tutors.razor`, `Pets.razor`) injeta **`ITutorStore`/`IPetStore`**, não `ApiClient`.

WASM: **`WebIndexedDbSqlitePersistence`** + `wwwroot/js/sqlite-db-storage.js`; **`SQLitePCL.Batteries_V2.Init()`**; **`MigrateAsync`** após restore. MAUI: path `FileSystem.AppDataDirectory/sysvet.db`, **`NoOpSqliteFilePersistence`**.

**Fonte da verdade na 3.4:** SQLite local. Dados existentes só na API **não** eram hidratados até a 3.5 (pull). Outbox de pet entregue na 3.5.

## Consequências

- **Positivas:** aceite 3.4 (CRUD offline); mesmo DTO/`Result` que a API; 61+ testes em `Clients.Tests`.
- **Negativas:** listas vazias até cadastro local; snapshot IndexedDB copia o arquivo inteiro (aceitável para CRM MVP).
- **Pendências:** PoC 3.6 (`sync-poc.md`, métricas E2E).

## Confirmação no código

- [`OfflineDbContext`](../../src/Clients/Clients.Infrastructure/OfflineDbContext.cs), migrations em [`Migrations/`](../../src/Clients/Clients.Infrastructure/Migrations/)
- [`AddClientPersistence`](../../src/Clients/Clients.Infrastructure/DependencyInjection/ClientPersistenceServiceCollectionExtensions.cs)
- Stores: [`OfflineTutorStore`](../../src/Clients/Clients.Infrastructure/Crm/OfflineTutorStore.cs), [`HttpTutorStore`](../../src/Clients/Clients.Infrastructure/Crm/HttpTutorStore.cs)
- WASM: [`WebIndexedDbSqlitePersistence`](../../src/Clients/BlazorWeb/Services/WebIndexedDbSqlitePersistence.cs), [`sqlite-db-storage.js`](../../src/Clients/BlazorWeb/wwwroot/js/sqlite-db-storage.js)
- MAUI boot: [`MauiProgram.cs`](../../src/Clients/MauiApp/MauiProgram.cs)

## Relacionados

- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-012](./ADR-012-blazor-pwa-jwt.md), [ADR-013](./ADR-013-maui-blazor-hybrid.md)
- [roadmap](../roadmap.md) § 3.4
