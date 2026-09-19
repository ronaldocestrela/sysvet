# Status do Projeto SysVet

**Data de Atualização:** 18/09/2026

Este documento contém o resumo de tudo o que foi construído até agora e serve de bússola para os desenvolvedores e IA saberem exatamente onde estamos no cronograma de desenvolvimento, evitando análises exaustivas a cada nova interação.

## 🏆 Sprints Concluídas

### Sprint 0 a 4: Fundação Core e Infraestrutura
Estas sprints pavimentaram a estrutura base (SaaS modular, CQRS, Offline-First).

### Fase 1 — DI modular na API (1.3)
- **Composição:** `AddApplicationModules()` na API; `Add*Module()` em `DependencyInjection.cs` de Core, Veterinary, Inventory, Sales (+ stubs Petshop/Fiscal).
- **HTTP:** `ResultExtensions` + `ResultEndpointFilter`; endpoints por módulo (`MapCoreEndpoints`, `MapVeterinaryEndpoints`, …).
- **Testes:** `ModuleRegistrationTests`, `ResultExtensionsTests`, `ResultEndpointFilterTests` em `API.IntegrationTests`.

### Fase 1 — Observabilidade e health checks (1.5)
- **Health:** `DatabaseHealthCheck` (`core-db`) no Core; check de processo `api`; rotas `/health` (JSON), `/health/live`, `/health/ready` via `HealthCheckExtensions`.
- **Rastreamento:** `CorrelationIdMiddleware` + console JSON com `IncludeScopes` em `appsettings.json`.
- **Testes:** `DatabaseHealthCheckTests`, `HealthCheckTests`, `CorrelationIdMiddlewareTests`, `CorrelationIdHttpTests` em `API.IntegrationTests` / `Core.Tests`.

### Fase 1 — ADRs e documentação arquitetural (1.6)
- **ADRs:** ADR-001 a ADR-005 em formato MADR (opções, decisão, confirmação em `src/`); índice em [`docs/arquitetura/README.md`](arquitetura/README.md).
- **Diagramas:** C4 contexto/containers/componentes da API + sequência de sync em [`docs/diagramas/`](diagramas/).
- **Consolidação:** `ADR_002_Sincronizacao.md` na raiz reduzido a ponte para o ADR canônico.

### Fase 1 — Configuração por ambiente (1.4)
- **Options:** `JwtSettings`, `TenancySettings`, `DatabaseOptions` (Core); `VeterinaryOptions`, `InventoryOptions`, `SalesOptions` com `ValidateOnStart()`.
- **Connection strings:** `ConnectionStrings:DefaultConnection` + overrides opcionais; helper `ConfigureModuleDatabase` (Sqlite/SqlServer).
- **Secrets:** `UserSecretsId` na API; Staging/Production sem segredos no Git; documentação em [`docs/arquitetura/configuracao.md`](arquitetura/configuracao.md).
- **Testes:** `DatabaseOptionsValidationTests`, `ConfigurationTests` (fail-fast Production, connection string efetiva).
- **Core Domain & Application:** Base Entity, AggregateRoot, Result Pattern, CQRS com MediatR (Logging, Validation e Transaction Behaviors). Entidades de base `Tutor` e `Pet`.
- **Tenancy e Banco de Dados:** Schemas SQL por tenant (`ITenantContext.SchemaName`, ADR-003) com `TenantAwareModelCacheKeyFactory` e query filters; migrations Core baseline `dbo`.
- **Fase 2.2 — EF Core Core:** Migration `InitialCore`, `CoreDbContextFactory`, repositórios, seed de roles no boot (`IdentityDataSeeder`).
- **Fase 2.3 — Identity, JWT e RBAC:** CQRS (`Login`, `Refresh`, `Register` dev, `GetCurrentUser`), refresh hash (`UserRefreshTokens`), policies RBAC, OpenAPI Bearer, testes E2E (`AuthEndpointsTests`, `AuthorizationTests`). ADR-007.
- **Fase 2.4 — CRM Tutores/Pets:** `CreateTutor`/`DeleteTutor`, pets com espécie obrigatória e tutor ativo, `PagedResult`, endpoints `/api/v1/tutors|pets`, migration soft delete, ADR-008, diagrama [`crm-tutor-pet.mmd`](diagramas/crm-tutor-pet.mmd).
- **Identity & Auth:** ASP.NET Core Identity no `CoreDbContext` (`AppUser.TenantId`), JWT via `JwtAccessTokenIssuer`, `TenantClaimMiddleware`.
- **Offline-first (Sync):** Padrão Transactional Outbox configurado com Testes de Integração End-to-End validando sincronia com banco local (SQLite).
- **Testes e CI/CD:** Suíte robusta usando `xUnit`, `FluentAssertions` e `WebApplicationFactory` com DB em memória/SQLite para testes E2E. Pipeline do GitHub Actions em funcionamento.

### Sprint 5 - Parte 01: Módulo Veterinary (Sub-entrega A: Agenda Clínica Unificada)
Iniciado o desenvolvimento dos módulos de negócio isolados.
- **Domínio:** Criado a raiz de agregação `Appointment` e Value Objects/Enums `AppointmentStatus`. Regras de negócio de verificação de datas retroativas, controle de estado, e controle de choque de horários na agenda (via `ScheduleSlot`).
- **Application:** Criados os Handlers `ScheduleAppointmentCommandHandler` e `RescheduleAppointmentCommandHandler`.
- **Infraestrutura:** Criado o `VeterinaryDbContext` específico do módulo, com sua própria migration (`InitialVeterinary`) separada do Core. Repositórios injetados no DI de forma isolada.
- **Endpoints:** Expostos em `/api/v1/appointments` usando Minimal APIs (MapGroup), protegidos por `.RequireAuthorization()`.
- **Testes E2E:** Setup corrigido no `WebApplicationFactory` para invocar os migrations de módulos satélites garantindo a integridade dos testes de integração. Build rodando **100% verde** (19 testes passando).
- **Sub-entrega B (Prontuário/Vacinas):** `MedicalRecord` e `VaccineDose` implementados com imutabilidade e proteção de integridade clínica. Integrações com `Hospitalization` e testes (GREEN).
- **Sub-entrega C (UI Blazor):** Protótipos visuais e páginas de listagem e agendamento de consultas consumindo `Mock API`.
 
### Sprint 5 - Parte 02: Módulo Inventory (Estoque e Produtos)
Iniciado o módulo de estoque.
- **Sub-entrega A (Domínio e Infraestrutura):** Implementadas entidades `Product`, `StockMovement` e a projeção agregada `ProductBalance` com testes garantindo bloqueio de saldo negativo (GREEN). Repositórios criados e banco gerado na migration `InitialInventory`.
- **Sub-entrega B (Aplicação):** Casos de Uso (Handlers MediatR) `RegisterProductCommand` e `RegisterStockMovementCommand` concluídos, testados (GREEN) e integrados com a unidade de trabalho isolada `IInventoryUnitOfWork`.

---

## 🚀 Onde Estamos e Próximos Passos

**Fase 3.3 (MAUI Blazor Hybrid)** concluída (ADR-013): Android + Windows, JWT/CRM via SharedUI (`ClientAuthState`, `AuthorizeRouteView`), branding VetNexus, CI `maui-publish` no Windows.

### Fase 3.4 (SQLite local nos clients) — Concluída (ADR-014)

- `OfflineDbContext` + migration `InitialOffline` (Tutors, Pets, OutboxMessages).
- `ITutorStore`/`IPetStore` offline; SharedUI tutor/pet sem dependência de rede para CRUD.
- WASM: IndexedDB snapshot; MAUI: `AppDataDirectory/sysvet.db`; testes `Clients.Tests` (61+).

### Fase 3.6 (PoC E2E offline → nuvem) — Concluída

- Cenário automatizado: `tests/API.IntegrationTests/Sync/OfflineToCloudPocTests.cs` (worker real + métricas).
- Documentação: [`docs/arquitetura/sync-poc.md`](arquitetura/sync-poc.md).

### Fase 4.1 (Agenda clínica unificada) — Concluída

- Domínio: `InProgress`, máquina de estados, `ScheduleSlot.Block`/`Book` com `Result`.
- API: CRUD agenda, slots, transições; sync plugin Veterinary no push/pull.
- Clients: `DayCalendar`, `IAppointmentStore` offline, migration `AddOfflineAppointments`.

### Fase 4.2 (Prontuário veterinário) — Concluída

- Domínio: anamnese, vitais, evolução, diagnóstico, conduta, finalize; 1:1 com appointment.
- API/CQRS: CRUD clínico + timeline; auditoria via `IAuditLogger`.
- Sync: medical records no push/pull e SQLite local.
- UI: `/pets/{petId}/medical-records`, link na agenda e lista de pets.

### Fase 4.3 (Exames, receitas e anexos) — Concluída

- Domínio: templates, receita emitida, exames, anexos (metadados + `BlobKey`).
- API: upload multipart, download stream autorizado; integração `IBlobStorage` (ADR-015).
- Sync: metadados 4.3 no plugin Veterinary; bytes só online.
- UI: exames, receitas e anexos em [`MedicalRecords.razor`](../src/Clients/SharedUI/Pages/MedicalRecords.razor).

### Fase 4.4 (Carteira de vacinação e alertas) — Concluída

- Domínio: protocolos por espécie/idade, `VaccineSchedule`, doses com próxima prevista; `Pet.BirthDate`.
- API/CQRS: protocolos, registro idempotente, carteira, query Overdue/Upcoming (horizon 7d).
- Sync: protocolos + doses no plugin Veterinary; outbox client para dose.
- UI: `/pets/{id}/vaccination-card`, `/vaccine-alerts`, menu `vaccines`; ADR-016.

### Fase 4.5 (Orçamentos clínicos) — Concluída

- Domínio: `ClinicalQuote`, máquina de estados, fila `ConversionStatus.Pending`, evento de integração.
- API/CQRS: CRUD de fluxo, inbox pending-conversions; ADR-017.
- Sync: pull + outbox client; SharedUI prontuário, print e `/clinical-quotes/pending`.

### Fase 4.6 (Internação e mapa de execução) — Concluída

- Domínio: recintos/leitos, ordens horárias, slots de administração, evolução e procedimentos internados.
- API/CQRS: mapa do dia, CRUD clínico; migration `AddHospitalizationExecutionMap`; ADR-018.
- Sync: pull ward/hospitalizations; outbox client; SQLite offline.
- UI: mapa por leito, detalhe, configuração de recintos (API online); testes bUnit.

### Fase 5.1 (Cadastro de produtos e lotes) — Concluída

- Domínio: `Supplier`, `ProductLot`, produto expandido (SKU, fiscais, custo médio).
- API/CQRS: CRUD catálogo + lotes; testes de aceite (dois lotes, saldo por lote).
- Sync: plugin Inventory no push/pull; SQLite offline + `IInventoryStore`.
- UI: `/products`, `/products/{id}`, `/suppliers` (SharedUI).

### Fase 5.2 — Movimentações e alertas — Concluída (ADR-020)

- Movimentações lot-aware, transferência, kardex, alertas; sync `StockMovement`; UI `/stock-movements`, `/stock-alerts`.

### Fase 5.3 — Entrada via XML (NF compra) — Concluída (ADR-021)

- Parser NF-e (`NfePurchaseXmlParser`), agregado `PurchaseInvoiceImport`, API parse/confirm, evento `PurchaseInvoiceImportedEvent` (AP Fase 7).
- Permissões `PurchaseImports.Read/Write`; UI online `/purchase-imports`.

### Fase 5.4 — Perdas, fracionamento e devoluções — Concluída (ADR-022)

- Domínio: `StockLossReasons`, `UnitsPerPackage`, lote `IsFractional`, `PackageFractionationService`.
- API/CQRS: perda, fracionamento, devolução ao fornecedor; evento `SupplierReturnRegisteredEvent`.
- Sync + UI offline: novos commands/campos; `/stock-movements` e detalhe de produto.

### Fase 5.5 — Inventário mobile (barcode) — Concluída (ADR-023)

- Sessão online `InventoryCount` (contagem cega → submit → approve); ajustes `InventoryCount` no ledger.
- API `/api/v1/inventory/counts`; UI `/inventory-counts`; scanner MAUI via `IBarcodeScannerService`.

### 👉 **Próxima Ação: Fase 5.6 — Etiquetas e sugestão de compras**

Ver [`roadmap.md`](roadmap.md) § Fase 5.

### Fase 3.5 (Motor de sincronização) — Concluída (ADR-002)

- Outbox completo tutor/pet/delete; `PushSyncBatchCommand` + `PullChangesQuery`; LWW; retry/dead-letter; `SyncBackgroundWorker` + `SetSyncing`.

**Nota:** MAUI build local exige workload; Linux CI continua via `SaaS_Veterinario.ci.slnf` (sem MAUI).

---

> **Regra Mestra do Projeto:** O TDD será estritamente seguido (Testes RED -> Implementação GREEN -> REFACTOR) e a sub-entrega B só será considerada concluída quando o build estiver limpo (0 warnings obstrutivos) e 100% dos testes rodarem em verde.
