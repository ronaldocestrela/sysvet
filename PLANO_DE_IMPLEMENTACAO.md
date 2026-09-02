# Plano de Implementação do SysVet

> Baseline verificado em 31/08/2026. Este documento foi escrito para permitir que qualquer desenvolvedor ou LLM continue o trabalho sem depender do histórico da conversa. A fonte de verdade é o código atual; `docs/roadmap.md` e `docs/structure.md` devem ser mantidos atualizados a cada Sprint.

---

## 1. Objetivo e Regras de Execução

O objetivo é transformar o scaffold atual em um SaaS veterinário modular, multi-tenant e offline-first conforme `docs/agents.md`, `docs/functions.md`, `docs/backoffice.md`, `docs/roadmap.md`, `docs/structure.md` e os ADRs em `docs/arquitetura/`.

Toda implementação deve obedecer rigorosamente a estas regras:

1. Usar .NET 10, ASP.NET Core, EF Core 10, SQL Server na nuvem, SQLite local, Blazor WASM PWA e MAUI Blazor Hybrid.
2. Preservar a direção de dependências: `Domain -> nenhuma infraestrutura`, `Application -> Domain`, `Infrastructure -> Application/Domain`, `API/Clients -> módulos`.
3. Aplicar CQRS com MediatR, Repository Pattern e `Result<T>` para falhas esperadas.
4. **TDD Obrigatório (Test-Driven Development):** Todo e qualquer desenvolvimento deve ser iniciado pela criação dos testes automatizados (unitários, de integração ou de contrato) que falham (RED). Apenas após os testes estarem criados a implementação funcional é realizada para torná-los verdes (GREEN), seguida de refatoração (REFACTOR).
5. **Regra de Salvaguarda (Gatekeeper de Transição):** A transição para a próxima Sprint (ou subgrupo/parte dentro de uma mesma Sprint) **SÓ PODERÁ OCORRER** quando a Sprint/subgrupo anterior estiver **100% CONCLUÍDA**, atendendo obrigatoriamente a todos os critérios:
   - Testes rodando em verde (`dotnet test SaaS_Veterinario.slnx` = 100% de sucesso).
   - Build limpo e sem erros (`dotnet build SaaS_Veterinario.slnx --no-restore` = Succeeded).
   - Ausência de vulnerabilidades de pacotes que impeçam a compilação (ex: `NU1903`).
   - Documentação viva sincronizada com o incremento entregue.
6. Adicionar XML docs aos tipos e membros públicos.
7. Não iniciar vários módulos de negócio simultaneamente. Fechar cada incremento vertical (do domínio à UI/endpoint) antes de ampliar escopo.

---

## 2. Diagnóstico: Estado Real do Projeto

| Área proposta | Estado real | Evidência e observação |
|---|---|---|
| Solução .NET modular | Concluído | `SaaS_Veterinario.slnx` contém API, Blazor WASM, SharedUI, MAUI, seis módulos em três camadas e suíte de testes integrada. |
| `src/API` | Concluído | OpenAPI/Scalar, endpoints de Auth, Core (Tutors, Pets), health checks `/health/live` e `/health/ready`, Middlewares (Correlation ID, TenantClaim, ExceptionHandler). |
| `src/Modules/Core/Domain` | Concluído | `Entity`, `AggregateRoot`, `Result`, erros de domínio, Value Objects (`Cpf`, `Email`, `Phone`), `Tutor`, `Pet`, repositórios, UoW, `ITenantContext` (TenantId, UserId, SchemaName). |
| `Core/Application` | Concluído | MediatR pipeline com `ValidationBehavior`, `LoggingBehavior`, `TransactionBehavior`, handlers para Tutors e Pets retornando `Result<T>`. |
| `Core/Infrastructure` | Concluído | `CoreDbContext` isolado por schema, EF Mappings, Repositórios, UoW, ASP.NET Core Identity, JWT Bearer generator, AuditLog append-only. |
| Multi-tenancy | Concluído | ADR-003 schema por tenant ativado via `IModelCacheKeyFactory`, `TenantClaimMiddleware` extraindo claim do JWT. |
| Blazor WASM & SharedUI | Concluído | PWA configurado em `BlazorWeb`, componentes Razor reutilizáveis em `SharedUI` (Layout, NavMenu, TutorForm, PetForm, Status Conectividade). |
| MAUI Hybrid | Concluído | `src/Clients/MauiApp/MauiApp.csproj` criado e integrado. |
| Sync Offline-First | Concluído | ADR-002 aceito com Transactional Outbox Pattern, `OfflineDbContext` (SQLite), `OutboxMessage`, worker de sincronização e testes E2E (`EndToEndSyncTests`) 100% verdes. |
| Veterinary, Petshop, Sales, Inventory, Fiscal | Scaffold | Estrutura de diretórios e projetos `.csproj` prontos, aguardando implementação funcional nas Sprints 5. |
| Finance, Automations, Intelligence, TutorPortal, Platform | Ausentes | Previstos nas Sprints 5 e 6. |
| Testes | 100% Verdes | 42 testes passando (1 skipped por exigir SQL Server em Dotmim.Sync). Suíte com testes de Handlers, CRUD, Auth, Tenancy, Multi-tenant isolation e Outbox Sync E2E. |
| CI | Concluído | `.github/workflows/ci.yml` configurado com restore, build e test. |
| Configuração | Concluído | Options tipados (`JwtSettings`, `TenancySettings`) com `ValidateOnStart()` fail-fast. |
| ADRs | Concluído | ADR-001, ADR-002 (Outbox Sync), ADR-003 (Multi-tenancy), ADR-004 (CQRS/MediatR) aceitos e documentados com diagramas em `docs/diagramas/sync-sequence.mmd`. |

---

## 3. Problemas Bloqueantes (Atuados e Resolvidos nas Sprints 0 a 4)

1. `NU1903` e `global.json` resolvidos no baseline.
2. `CoreDbContext.TenantContext` injetado e schema dinâmico configurado.
3. Injeções de DI de repositórios, UoW, MediatR e behaviors concluídas.
4. Segredos parametrizados em Options tipados com `ValidateOnStart()`.
5. Implementação do `OutboxPattern` offline-first com testes E2E verdes.
6. Ajuste da propriedade `UserId` em `ITenantContext` e mocks de teste.

---

## 4. Divisão das Tarefas em Sprints (Ordem de Criticidade e Dificuldade)

> **[SALVAGUARDA DE TRANSIÇÃO]** Nenhuma próxima Sprint ou subgrupo pode ser iniciado sem a conclusão do subgrupo atual com testes rodando em verde e build OK.

### **Sprint 0: Estabilização do Baseline, Build e CI** — [CONCLUÍDA]
### **Sprint 1: Fundação Arquitetural do Core** — [CONCLUÍDA]
### **Sprint 2: Primeiro Incremento Vertical — CRUD Tutor & Pet** — [CONCLUÍDA]
### **Sprint 3: Autenticação, RBAC e Auditoria** — [CONCLUÍDA]
### **Sprint 4: Clientes Base & PoC Offline-First (ADR-002)** — [CONCLUÍDA]

---

### **Sprint 5: Módulos Operacionais do MVP**
*Criticidade: ALTA | Dificuldade: ALTA*

*   **Sprint 5 - Parte 01: Módulo Veterinary (Agenda, Consulta e Prontuário)**
    1. **[TDD Estrito]** Testes de domínio para reagendamento, conflito de horários, regras de prontuário e LGPD clínica.
    2. Implementar Domínio, Casos de Uso, Persistência EF, Migrations, Endpoints e UI para Agenda, Consulta, Prontuário, Vacinas e Internação.
*   **Sprint 5 - Parte 02: Módulo Inventory (Estoque e Insumos)**
    1. **[TDD Estrito]** Testes de domínio para cálculo de saldo derivado, controle de validade/lotes e imutabilidade de movimentações.
    2. Implementar Domínio, Casos de Uso, Persistência, Endpoints e UI para Produtos, Lotes/Validade, Fornecedores e Inventário por Barcode.
*   **Sprint 5 - Parte 03: Módulo Sales (Vendas, Caixa e PDV Offline)**
    1. **[TDD Estrito]** Testes de domínio para fechamento de caixa, cálculo de troco/desconto com Value Object `Money` e idempotência de vendas.
    2. Implementar Pedidos, Caixa, PDV Offline com sincronização, Endpoints e UI.
*   **Sprint 5 - Parte 04: Módulo Petshop (Serviços e Banho/Tosa)**
    1. **[TDD Estrito]** Testes para pacotes pré-pagos e baixa automática de estoque de insumos no módulo `Inventory`.
    2. Implementar Agendamento Estético, Fichas de Atendimento e Consumo de Insumos.
*   **Sprint 5 - Parte 05: Módulo Finance (Financeiro e Conciliação)**
    1. **[TDD Estrito]** Testes de integração entre eventos de vendas/serviços e o financeiro.
    2. Criar `src/Modules/Finance` (Contas a Pagar/Receber, Fluxo de Caixa, Conciliação e DTOs de integração).
*   **Sprint 5 - Parte 06: Módulo Fiscal (Emissão de Notas e Contingência)**
    1. **[TDD Estrito]** Testes de mensageria fiscal e geração de XML de contingência offline.
    2. Elaborar ADR-007 (Provedor Fiscal).
    3. Implementar NF-e / NFC-e / NFS-e, assinatura digital, contingência offline e transmissão SEFAZ.

---

### **Sprint 6: Relacionamento, Portal do Tutor e Plataforma SaaS**
*Criticidade: MÉDIA | Dificuldade: ALTA*

*   **Sprint 6 - Parte 01: Módulo Automations (Comunicação e Notificações)**
    1. **[TDD Estrito]** Testes de filas Outbox de notificações e controle de opt-out/consentimento do tutor.
    2. Criar `src/Modules/Automations` (WhatsApp, SMS, E-mail e Lembretes automáticos).
*   **Sprint 6 - Parte 02: Módulo TutorPortal (Portal do Cliente)**
    1. **[TDD Estrito]** Testes de autorização por vínculo Tutor-Pet.
    2. Criar `src/Modules/TutorPortal` (Carteira de vacinas, exames e agendamento online).
*   **Sprint 6 - Parte 03: Módulo Platform & Intelligence (SaaS & Analytics)**
    1. **[TDD Estrito]** Testes de provisionamento automático de schema e impersonation auditada.
    2. Criar `src/Modules/Platform` (Onboarding, Billing, Feature Flags, Tenants/Filiais).
    3. Criar `src/Modules/Intelligence` (Dashboards tenant e agregados de métricas SaaS).
    4. Criar projeto `tests/Architecture.Tests` com NetArchTest para imposição de dependências de arquitetura.

---

### **Sprint 7: Operação, Segurança, Produção e Hardening**
*Criticidade: MÉDIA | Dificuldade: ALTA*

*   **Sprint 7 - Parte 01: Infraestrutura de Produção e Observabilidade**
    1. **[TDD / Testes]** Testes automatizados de Liveness/Readiness e métricas OpenTelemetry.
    2. Dockerização da API, pipeline de Migrations e estratégia de Backup/Disaster Recovery.
    3. OpenTelemetry, traces, métricas e alertas operacionais.
*   **Sprint 7 - Parte 02: Segurança LGPD, Carga e Hardening**
    1. **[TDD / Testes]** Testes de carga, estresse e concorrência de estoque/caixa e caos de rede durante sync.
    2. Threat Modeling, retenção/anonimização LGPD, Rate Limiting e cabeçalhos de segurança.
    3. Confecção dos Runbooks operacionais.

---

## 5. Estrutura de Pastas e Módulos Alvo

```text
src/Modules/{Finance,Automations,Intelligence,TutorPortal,Platform}/
  Domain/
  Application/
  Infrastructure/
  README.md
src/Clients/MauiApp/
tests/Modules/{Finance,Automations,Intelligence,TutorPortal,Platform}.Tests/
tests/Architecture.Tests/
docs/diagramas/
  c4-context.mmd
  c4-containers.mmd
  core-erd.mmd
  sync-sequence.mmd
docs/arquitetura/
  configuracao.md
  ADR-005-persistencia-por-ambiente.md
  ADR-006-integracao-entre-modulos.md
  ADR-007-provedor-fiscal.md
```

---

## 6. Checklist de Qualidade Obrigatório por Tarefa

- [ ] **[TDD]** Teste que falha (RED) criado antes da implementação.
- [ ] Domínio isolado sem dependência de EF Core, ASP.NET Core, UI ou infraestrutura externa.
- [ ] Operação utiliza CQRS (Command ou Query) e retorna `Result`/`Result<T>`.
- [ ] Multi-tenancy isolado via `ITenantContext` e filtrado na persistência.
- [ ] Operações de escrita são idempotentes para suportar retries e sincronização offline.
- [ ] DTOs públicos não expõem entidades do EF Core nem dados sensíveis (CPF/e-mail) sem tratamento.
- [ ] Logs e auditorias sanitizados (sem tokens, senhas ou prontuários completos).
- [ ] Suíte de testes (unitários, integração, E2E) 100% verde (`dotnet test`).
- [ ] Migrations revisadas com estratégia de rollout e rollback.
- [ ] Documentação, XML docs e `docs/roadmap.md` atualizados.
- [ ] `dotnet format --verify-no-changes`, build e testes sem alertas ou erros.

---

## 7. Comandos de Verificação e Gatekeeper

Executar a partir da raiz da solução:

```bash
dotnet --info
dotnet restore SaaS_Veterinario.slnx
dotnet build SaaS_Veterinario.slnx --no-restore --configuration Release
dotnet test SaaS_Veterinario.slnx --no-build --configuration Release --collect:"XPlat Code Coverage"
dotnet format SaaS_Veterinario.slnx --verify-no-changes
dotnet list src/API/API.csproj package --include-transitive --vulnerable
```

Comando para criação de Migrations (quando aplicável):
```bash
dotnet ef migrations add NomeDaMigration --project src/Modules/Core/Infrastructure/Core.Infrastructure.csproj --startup-project src/API/API.csproj
```

---

## 8. Próximo Passo Exato para a LLM / Desenvolvedor Executores

Iniciar imediatamente a **Sprint 5 - Parte 01: Módulo Veterinary**:
1. Criar testes unitários de domínio e aplicação em `Veterinary.Tests` para agendamentos, consultas, prontuários, vacinas e internação.
2. Implementar entidades de domínio `Appointment`, `MedicalRecord`, `VaccineCard`, `Hospitalization` em `Veterinary.Domain`.
3. Implementar Commands/Queries CQRS com FluentValidation em `Veterinary.Application`.
4. Mapear EF Core e Migrations em `Veterinary.Infrastructure`.
5. Registrar modulo em `src/API/Extensions/VeterinaryModuleExtensions.cs` e expor endpoints RESTful `/api/v1/veterinary`.
6. Criar componentes Razor em `SharedUI` para prontuário e agendamentos.
