# Status do Projeto SysVet

**Data de Atualização:** 16/09/2026

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

**Fase 2.6 (Auditoria e contratos de API)** concluída: captura `IAuditable`, `GET /api/v1/audit-logs`, Problem Details + `correlationId`, OpenAPI v1 e tags por módulo, ADR-010. Próximo marco: **Fase 3 — Clientes e Offline-First**.

Estamos na **Sprint 5 - Parte 02 (Módulo Inventory)**.

As Sub-entregas A e B estão finalizadas. O alicerce do banco de dados, domínio e aplicação estão prontos.

### 👉 **Próxima Ação: Sprint 5 - Parte 02 (Sub-entrega C: Endpoints e UI)**

**O que faremos:**
1. **API Endpoints:** Criar `InventoryEndpoints.cs` utilizando Minimal APIs para expor rotas como `POST /api/v1/products` e `POST /api/v1/stock/movements`.
2. **Integração E2E:** Criar `ProductEndpointsTests.cs` (usando `WebApplicationFactory` e autenticação JWT mockada) para validar os endpoints ponta a ponta.
3. **UI/Frontend:** Construir as páginas Blazor WASM correspondentes para exibir os produtos, saldo em estoque, e um modal para lançamento de entrada/saída, consumindo uma Mock API (conforme convenção do projeto para testes visuais antecipados).

---

> **Regra Mestra do Projeto:** O TDD será estritamente seguido (Testes RED -> Implementação GREEN -> REFACTOR) e a sub-entrega B só será considerada concluída quando o build estiver limpo (0 warnings obstrutivos) e 100% dos testes rodarem em verde.
