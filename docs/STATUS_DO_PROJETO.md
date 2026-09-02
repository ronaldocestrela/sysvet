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

---

## 🚀 Onde Estamos e Próximos Passos

Estamos na **Sprint 5 - Parte 01 (Módulo Veterinary)**.

A **Sub-entrega A (Agenda)** está finalizada. A partir de agora, o próximo passo imediato é:

### 👉 **Próxima Ação: Sprint 5 - Parte 01 (Sub-entrega B: Prontuário e Carteira de Vacinação)**

**O que faremos:**
1. **Domínio (TDD Estrito):** Criação das Entidades `MedicalRecord` (Prontuário de Atendimento) e `VaccineCard` (Carteira de Vacinação). As regras de domínio preverão imutabilidade de certas anotações clínicas após finalizadas (para não ferir LGPD e compliance de CRMV) e registro de lote/data de aplicação de vacinas.
2. **Application:** Comandos para `CreateMedicalRecord`, `UpdateMedicalRecordNotes`, `AddVaccineDose`.
3. **Infraestrutura:** Atualização do `VeterinaryDbContext`, mapeamento EF Core, e nova Migration `AddMedicalRecords`. O Id da consulta original (`AppointmentId`) servirá de vínculo para o Prontuário.
4. **Endpoints:** `/api/v1/appointments/{id}/records` e `/api/v1/pets/{id}/vaccines`.
5. **Integração:** Adicionar as chamadas E2E para garantir o funcionamento com Autenticação e Multi-tenancy.

---

> **Regra Mestra do Projeto:** O TDD será estritamente seguido (Testes RED -> Implementação GREEN -> REFACTOR) e a sub-entrega B só será considerada concluída quando o build estiver limpo (0 warnings obstrutivos) e 100% dos testes rodarem em verde.
