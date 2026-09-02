# Status do Projeto SysVet

**Data de Atualização:** 02/09/2026

Este documento contém o resumo de tudo o que foi construído até agora e serve de bússola para os desenvolvedores e IA saberem exatamente onde estamos no cronograma de desenvolvimento, evitando análises exaustivas a cada nova interação.

## 🏆 Sprints Concluídas

### Sprint 0 a 4: Fundação Core e Infraestrutura
Estas sprints pavimentaram a estrutura base (SaaS modular, CQRS, Offline-First).
- **Core Domain & Application:** Base Entity, AggregateRoot, Result Pattern, CQRS com MediatR (Logging, Validation e Transaction Behaviors). Entidades de base `Tutor` e `Pet`.
- **Tenancy e Banco de Dados:** Múltiplos schemas (Isolation por Tenant) dinâmicos usando EF Core Interceptors e `IModelCacheKeyFactory`.
- **Identity & Auth:** ASP.NET Core Identity isolado no `CoreDbContext`. Criação de rotas `/api/v1/auth/login`, geração de JWT. Configuração de `TenantClaimMiddleware`.
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

Estamos na **Sprint 5 - Parte 02 (Módulo Inventory)**.

As Sub-entregas A e B estão finalizadas. O alicerce do banco de dados, domínio e aplicação estão prontos.

### 👉 **Próxima Ação: Sprint 5 - Parte 02 (Sub-entrega C: Endpoints e UI)**

**O que faremos:**
1. **API Endpoints:** Criar `InventoryEndpoints.cs` utilizando Minimal APIs para expor rotas como `POST /api/v1/products` e `POST /api/v1/stock/movements`.
2. **Integração E2E:** Criar `ProductEndpointsTests.cs` (usando `WebApplicationFactory` e autenticação JWT mockada) para validar os endpoints ponta a ponta.
3. **UI/Frontend:** Construir as páginas Blazor WASM correspondentes para exibir os produtos, saldo em estoque, e um modal para lançamento de entrada/saída, consumindo uma Mock API (conforme convenção do projeto para testes visuais antecipados).

---

> **Regra Mestra do Projeto:** O TDD será estritamente seguido (Testes RED -> Implementação GREEN -> REFACTOR) e a sub-entrega B só será considerada concluída quando o build estiver limpo (0 warnings obstrutivos) e 100% dos testes rodarem em verde.
