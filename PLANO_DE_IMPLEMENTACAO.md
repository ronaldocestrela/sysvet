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
| Solução .NET modular | Parcial | `SaaS_Veterinario.slnx` contém API, Blazor, SharedUI, seis módulos em três camadas e projetos de teste. MAUI não está incluído. |
| `src/API` | Parcial | OpenAPI/Scalar em Development, `/health`, rota raiz e grupo Core vazio. Não há endpoints funcionais de negócio, autenticação ou tradução de `Result` para HTTP. |
| `src/Modules/Core/Domain` | Parcial avançado | `Entity`, `AggregateRoot`, `Result`, erros, VOs, `Tutor`, `Pet`, repositórios, UoW e tenant context existem. Faltam eventos, auditoria, atualização das entidades e contratos adicionais. |
| `Core/Application` | Parcial | Contratos CQRS e behaviors de logging/validação existem. Não há handlers, DTOs, validators de features, mapeamento, autorização nem registro MediatR na DI. |
| `Core/Infrastructure` | Parcial | DbContext, mappings, migration e repositórios existem. A DI não registra repositórios/UoW; o tenant context não é injetado no construtor do DbContext; não há Identity/JWT. |
| Multi-tenancy | Scaffold incorreto/incompleto | ADR-003 escolhe schema por tenant. `CoreDbContext` expõe `TenantContext`, mas ele nunca é atribuído por DI. SQLite ignora schema e a API usa conexão fixa `sysvet.db`. Não há resolução por claim nem cache key de modelo por schema. |
| Blazor WASM | Scaffold | Projeto compila como template; ainda contém Counter/Weather e não possui autenticação, API client, PWA/offline funcional ou telas do domínio. |
| SharedUI | Scaffold | Razor Class Library existe, mas contém `Component1` e exemplo de JS interop. |
| MAUI | Ausente | Só há `src/Clients/MauiApp/README.md` e `.gitkeep`; não existe `.csproj`. |
| Veterinary, Petshop, Sales, Inventory, Fiscal | Scaffold | Projetos e referências existem, porém não possuem domínio, casos de uso ou persistência. READMEs descrevem intenção futura. |
| Finance, Automations, Intelligence, TutorPortal, Platform | Ausentes | Previstos no roadmap e documentos funcionais, mas não existem em `src/Modules`. |
| Testes | Parcial | Core possui 25 métodos `[Fact]` e 8 `[Theory]`; há teste de `/health`. Os demais projetos contêm `UnitTest1` vazio. Não há testes de handlers, CRUD, auth, tenancy ou sync. |
| CI | Parcial | `.github/workflows/ci.yml` executa restore/build/test e gera relatório, mas não publica artefato, não impõe cobertura, não tem badge e ainda precisa ser validado após corrigir o build. |
| Configuração | Parcial/insegura | Arquivos por ambiente existem, mas contêm connection strings e segredos de exemplo; o código ignora essas conexões e usa SQLite fixo. Não há Options tipados nem validação no startup. |
| ADRs | Parcial | ADR-001, 003 e 004 estão aceitos; ADR-002 exige PoC. Faltam diagramas C4/ERD/fluxos, ADR de persistência por ambiente e detalhamento operacional das decisões. |
| Documentação | Desalinhada | `docs/roadmap.md` ainda diz que CI, EF, CQRS, config e ADRs não existem. README raiz aponta para `./agents.md`, que não existe; o correto é `docs/agents.md`. |

---

## 3. Problemas Bloqueantes (Atuados na Sprint 0)

1. `dotnet build SaaS_Veterinario.slnx --no-restore` falha no baseline por `NU1903` (`Microsoft.OpenApi 2.0.0`).
2. Resolução de `global.json` para alinhamento do SDK .NET 10.
3. Tratamento de exceção de Socket no VSTest durante execução de auditoria.
4. `CoreDbContext.TenantContext` não inicializado pelo construtor (schema caindo em `dbo`).
5. `AddCoreModule()` com registros de DI incompletos (faltam repositórios, UoW, MediatR e pipeline behaviors).
6. Segredos expostos em `appsettings.Staging.json` e `appsettings.Production.json`.

---

## 4. Divisão das Tarefas em Sprints (Ordem de Criticidade e Dificuldade)

> **[SALVAGUARDA DE TRANSIÇÃO]** Nenhuma próxima Sprint ou subgrupo pode ser iniciado sem a conclusão do subgrupo atual com testes rodando em verde e build OK.

### **Sprint 0: Estabilização do Baseline, Build e CI**
*Criticidade: CRÍTICA | Dificuldade: MÉDIA*

*   **Sprint 0 - Parte 01: Correção de Dependências e Build Limpo**
    1. **[TDD / Verificação]** Criar script/teste de verificação da integridade de build/restore.
    2. Resolver a vulnerabilidade transitiva `NU1903` (`Microsoft.OpenApi 2.0.0` trazida por Scalar/OpenAPI).
    3. Criar `global.json` na raiz da solução fixando o SDK .NET 10.
    4. Validar compilação limpa com `dotnet build SaaS_Veterinario.slnx --no-restore`.
*   **Sprint 0 - Parte 02: Limpeza, Alinhamento de Documentação e CI**
    1. **[TDD / Testes]** Garantir que nenhum comportamento seja removido sem testes de regressão.
    2. Limpar placeholders (`UnitTest1.cs`, `Counter`, `Weather`, `Component1`, `ExampleJsInterop`) apenas quando houver substitutos.
    3. Atualizar links do `README.md` raiz para `docs/agents.md`.
    4. Atualizar `docs/roadmap.md` e `docs/structure.md` alinhados com `.slnx`.
    5. Atualizar `.github/workflows/ci.yml` (restore explícito, `--no-restore` no build, `--no-build` nos testes, coleta de cobertura XPlat).
    6. Marcar `implementation_plan.md` e `status_e_proximos_passos.md` locais como históricos.

---

### **Sprint 1: Fundação Arquitetural do Core**
*Criticidade: ALTA | Dificuldade: ALTA*

*   **Sprint 1 - Parte 01: Multi-Tenancy e Isolamento de Persistência**
    1. **[TDD Estrito]** Escrever testes unitários de isolamento multi-tenant (garantir que Tenant A não acesse dados do Tenant B no `CoreDbContext`).
    2. Implementar contrato `ITenantContext` Scoped (`TenantId`, `SchemaName`).
    3. Injetar `ITenantContext` no construtor do `CoreDbContext`.
    4. Implementar `IModelCacheKeyFactory` registrando schema/tenant na chave de modelo do EF.
    5. Configurar provedores de banco por ambiente (SQL Server na nuvem/API, SQLite no cliente/local, InMemory/SQLite em memória nos testes).
    6. Registrar `ITutorRepository`, `IPetRepository` e `IUnitOfWork` na DI.
*   **Sprint 1 - Parte 02: Pipeline CQRS e Mapeamento de Erros**
    1. **[TDD Estrito]** Criar testes unitários para a pipeline MediatR (`ValidationBehavior`, `LoggingBehavior`, `TransactionBehavior`).
    2. Criar método de extensão `AddApplication()` em `Core.Application.DependencyInjection`.
    3. Implementar `ValidationBehavior` para tratar `Result` e `Result<T>`.
    4. Implementar behavior transacional exclusivo para Commands.
    5. Mapear erros de domínio/validação para `ProblemDetails` HTTP (400, 401, 403, 404, 409, 500).
*   **Sprint 1 - Parte 03: Observabilidade, Options e Health Checks**
    1. **[TDD Estrito]** Escrever testes para validação de startup com Options inválidos e endpoints de health.
    2. Criar Options tipados (`JwtSettings`, opções de tenancy) com `ValidateDataAnnotations()` e `ValidateOnStart()`.
    3. Adicionar Correlation/Trace ID aos logs estruturados e respostas HTTP.
    4. Expandir Health Checks separando `/health/live` e `/health/ready`.
    5. Criar documento `docs/arquitetura/configuracao.md`.

---

### **Sprint 2: Primeiro Incremento Vertical — CRUD Tutor & Pet**
*Criticidade: ALTA | Dificuldade: MÉDIA*

*   **Sprint 2 - Parte 01: Domínio, Regras de Negócio e Casos de Uso**
    1. **[TDD Estrito]** Escrever testes unitários de Domínio e Aplicação para `Create/Update/Get/ListTutor` e `Create/Update/Get/ListPet` (regras de CPF válido, e-mail único, pet duplicado e tutor inexistente).
    2. Adicionar métodos de mutação com setters privados nas entidades `Tutor` e `Pet`.
    3. Criar DTOs de Request/Response e Mappings seguros.
    4. Implementar Commands, Queries, Validators e Handlers retornando `Result<T>`.
*   **Sprint 2 - Parte 02: Endpoints RESTful, Idempotência e Integração**
    1. **[TDD Estrito]** Escrever testes de integração HTTP usando `WebApplicationFactory` com banco isolado por teste.
    2. Mapear endpoints em `src/API/Endpoints/Core` (`/api/v1/tutors`, `/api/v1/pets`).
    3. Adicionar controle de Idempotência para operações de escrita.

---

### **Sprint 3: Autenticação, RBAC e Auditoria**
*Criticidade: ALTA | Dificuldade: ALTA*

*   **Sprint 3 - Parte 01: Identity, JWT e Tenant Claim Middleware**
    1. **[TDD Estrito]** Escrever testes de login, falha de senha, expiração de token e rotação de refresh token.
    2. Configurar ASP.NET Core Identity no DbContext associado ao `TenantId`.
    3. Implementar endpoints de Login, Refresh Token rotativo/revogável e `/api/v1/auth/me`.
    4. **[TDD Estrito]** Escrever testes para o Middleware de Tenancy (validando rejeição de cabeçalhos manipulados pelo cliente e isolamento via claim JWT).
    5. Criar Middleware que extrai `TenantId` da claim do JWT e popula o `ITenantContext`.
*   **Sprint 3 - Parte 02: Autorização RBAC e Trilha de Auditoria**
    1. **[TDD Estrito]** Escrever testes de autorização HTTP (401/403) para endpoints Tutor/Pet contra as roles `Admin`, `Veterinarian`, `Receptionist`, `Cashier`.
    2. Implementar autorização baseada em Roles e Policies.
    3. **[TDD Estrito]** Escrever testes do mecanismo de Auditoria append-only.
    4. Criar entidade e serviço `AuditLog` append-only para login, mutações e alterações de permissão.

---

### **Sprint 4: Clientes Base & PoC Offline-First (ADR-002)**
*Criticidade: ALTA | Dificuldade: MUITO ALTA*

*   **Sprint 4 - Parte 01: SharedUI & Client Blazor WASM PWA**
    1. **[TDD Estrito]** Criar testes bUnit para componentes básicos de UI.
    2. Construir Design Tokens, componentes acessíveis e layout base em `SharedUI`.
    3. Criar client HTTP tipado com tratamento padronizado de `ProblemDetails` e gestão de tokens JWT.
    4. Desenvolver telas funcionais de Tutor e Pet no Blazor WASM.
    5. Configurar Service Worker PWA e política de atualização offline.
*   **Sprint 4 - Parte 02: Cliente MAUI Blazor Hybrid**
    1. **[TDD Estrito]** Criar testes de integração local do banco SQLite no ambiente mobile/desktop.
    2. Instalar workloads e criar `src/Clients/MauiApp/MauiApp.csproj` integrado à solução.
    3. Configurar Secure Storage nativo para tokens e detecção de conectividade.
*   **Sprint 4 - Parte 03: PoC de Sincronização & Fechamento do ADR-002**
    1. **[TDD Estrito]** Escrever testes E2E do fluxo offline completo (Offline -> SQLite -> Outbox -> Reconexão -> API -> SQL Server -> Resolução de Conflitos).
    2. Realizar PoC comparativa entre `Dotmim.Sync` e `Outbox/Inbox` manual.
    3. Definir uso de UUIDs no cliente, ETags/versão de registro, tombstones e política determinística de conflito por entidade.
    4. Fechar o `ADR-002` ("Aceito") e gerar diagramas de sequência em `docs/diagramas/sync-sequence.mmd`.

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

Iniciar imediatamente a **Sprint 0 - Parte 01**:
1. Diagnóstico e resolução da vulnerabilidade `NU1903`.
2. Criação do `global.json` com SDK .NET 10.
3. Validação do build e testes limpos.
4. Aplicação da Regra de Salvaguarda para avançar à **Sprint 0 - Parte 02**.
