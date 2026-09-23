# Status do Projeto SysVet

**Data de Atualização:** 19/09/2026

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
- **Identity & Auth:** ASP.NET Core Identity no `CoreDbContext` (`AppUser.TenantId`), JWT via `JwtAccessTokenIssuer`, `TenantResolutionMiddleware` (ADR-046).
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

### Fase 5.6 — Etiquetas e sugestão de compras — Concluída (ADR-024)

- `Product.TargetStock`; sugestão agrupada por fornecedor; export CSV; etiquetas PDF (QuestPDF) e ZPL na API.
- UI: `/purchase-suggestions`, geração de etiqueta no detalhe do produto (online).

### Fase 6.1 — Motor de vendas (PDV) — Concluída (ADR-025)

- Carrinho produto/serviço, split de pagamentos, tutor/pet, comprovante; caixa aberto/fechado + saldo derivado.
- Pay atômico com `ConsumeStockForSaleRequest`; orçamento aprovado → `ClinicalQuoteConvertedEvent`.
- Clients: checkout via HTTP na 6.1; catálogo local via `IInventoryStore`.

### Fase 6.2 — PDV 100% offline — Concluída (ADR-026)

- `ISalesStore`/`OfflineSalesStore`: SQLite + outbox Create+Pay; UUID de pedido/caixa no cliente; débito otimista de estoque local.
- Plugin Sales no push/pull; conflito permanente quando o servidor recusa estoque; `RequestSync()` após mutações.
- SharedUI POS/caixa/comprovante sem gate de rede; aceite `PdvOfflineTenSalesSyncTests` (10 vendas, replay idempotente).

### Fase 6.3 — Pagamentos e TEF — Concluída (ADR-027)

- Porta `IPaymentTerminal` + simulador offline; NSU em débito/crédito/Pix; estorno parcial/total; caixa líquido por forma.
- API `POST .../payments/{id}/refund`; sync outbox/pull; POS/comprovante/caixa SharedUI.

### Fase 6.4 — Comissões, descontos, devoluções — Concluída (ADR-028)

- Desconto no pedido com teto `AccessProfile.MaxDiscountPercent` (`/auth/me`); comissões snapshot no pay; devolução com estoque (`SaleReturn`) + estorno proporcional.
- API returns/commission-rules/commissions; sync pull/push; PDV offline com regras locais e UI comprovante **Devolver**.

### Fase 6.5 — Pacotes, kits e pré-pagos — Concluída (ADR-029)

- Kits de produtos (explosão de estoque no pay); pacotes pré-pagos com saldo por pet; consumo via API/`ConsumePrepaidServicePackageRequest`.
- Aceite: `PayPackageThenConsume_DecrementsRemainingUses` em `SalesEndpointsTests`.

### Fase 6.6 — Estética banho e tosa — Concluída (ADR-030)

- Módulo Petshop: agenda groomers, ficha B&T, catálogo de serviços, baixa de insumos na conclusão.
- API `/api/v1/grooming-*`; sync push/pull; clientes offline (`IGroomingStore`, `/grooming`).
- Aceite: `CompleteGrooming_DebitsConfiguredSupplies` em `GroomingEndpointsTests`.

### Fase 6.7 — Notificações de status (banho) — Concluída (ADR-031)

- Status `ReadyForPickup`, endpoint `/ready`, SignalR no tenant, porta `ITutorNotificationChannel` → fila Automations (8.1).
- Aceite: `MarkReady_WhenAutomationsChannelEnabled_NotifiesTutor`.

### Fase 7.1 — Módulo Finance (estrutura) — Concluída

- `src/Modules/Finance/{Domain,Application,Infrastructure}`; `FinanceDbContext`, `AddFinanceModule`.
- Migration `InitialFinance`; testes `FinanceDbContextTests`, `ModuleRegistrationTests`.
- Schema lógico do módulo no tenant schema (ADR-003).

### Fase 7.2 — Contas a pagar e receber — Concluída (ADR-032)

- Títulos AP/AR, categorias, centros de custo; integração `OrderPaidEvent` / `PurchaseInvoiceImportedEvent`.
- Endpoints Finance; sync pull/push; clientes `/finance` + `IFinanceStore`.
- Aceite: `PayOrder_ShouldDebitStockAndMarkFinanceLinked`, `ConfirmPurchaseXml_CreatesPayablesFromDuplicates`.

### Fase 7.3 — Caixa, sangrias e conciliação — Concluída (ADR-033)

- Caixa operacional no Sales: sangrias/suprimentos, saldo esperado na gaveta, fechamento com variance (não bloqueia).
- Finance: NSU nas alocações AR de cartão; import PoC de extrato e match por NSU + valor.
- Sync offline de movimentos; conciliação de cartão online-only; UI caixa + `/finance/card-reconciliation`.
- Aceite: `CloseCashRegister_ExpectedBalance_MatchesCashSalesMinusDrops`; `ImportCardStatement_MatchesReceivableByNsu`.

### Fase 7.4 — Fluxo de caixa e demonstrativos — Concluída (ADR-034)

- Fluxo de caixa (regime de caixa) e DRE simplificada (competência por emissão) derivados de AP/AR.
- API `cash-flow`, `dre`, `statements/export` (CSV/PDF); página `/finance/reports`; CSV offline + PDF online.
- Aceite: `MonthlyStatements_MatchAccountsPayableReceivable`; `ExportMonthlyStatements_ReturnsCsv` / `_ReturnsPdf`.

### Fase 7.5 — NF-e e NFS-e — Concluída (ADR-035)

- NF-e (Zeus.Net) e NFS-e Padrão Nacional (`OpenAC.Net.NFSe.Nacional.Web`); emissão explícita de pedido pago; cancelamento + CC-e (NF-e).
- `FiscalDbContext`, certificado A1 cifrado, API `/api/v1/fiscal*`, UI `/fiscal` + “Emitir nota” no comprovante; CI com `Fiscal:Provider=Fake`.
- Aceite homologação SEFAZ: teste opcional com `FISCAL_HOMOLOGATION=1` (fora do CI).

### Fase 7.6 — NFC-e e contingência offline — Concluída (ADR-036)

- NFC-e modelo 65 no PDV: contingência `tpEmis=9`, XML assinado localmente, QR/cupom no comprovante, outbox `TransmitNfceCommand` após pay.
- Cache A1 + emitente via `GET /api/v1/fiscal/issuer/pos-bundle`; sync push/pull fiscal; reconciliação SEFAZ.
- Aceite: `OfflineSale_EmitsNfceInContingency_TransmitsAfterSync` (CI com `FakeNfceGateway`).

### Fase 7.7 — Planejamento fiscal — Concluída (ADR-037)

- Apuração por período (autorização/cancelamento), CFOP, ISS estimado; simulação Simples vs Presumido (gerencial).
- API `GET /api/v1/fiscal/planning` e `/planning/export` (CSV/PDF); UI `/fiscal/planning` (online-only).
- Aceite: `PeriodTaxReport_MatchesAuthorizedDocuments`; `ExportFiscalPlanning_ReturnsCsv` / `_ReturnsPdf`.

### Fase 8.1 — Módulo Automations — Concluída (ADR-038)

- `src/Modules/Automations/`, outbox SQL, `OutboxProcessor`, templates por canal, API `/api/v1/automations`, UI `/automations`.
- `EnqueueingTutorNotificationChannel`; aceite `EnqueueJob_ThenProcessor_SucceedsWithAttemptLog`.

### Fase 8.2 — Lembretes WhatsApp/e-mail — Concluída (ADR-039)

- `ReminderScheduler`, gatilhos vacina D-7 / consulta D-1 / aniversário / retorno (`FollowUpOn`), opt-out tutor, horário comercial.
- Provedores: SMTP + Evolution API (`Automations:Provider` Fake/Live); SMS não enfileirado.
- Aceite: `VaccineReminder_EnqueuedSevenDaysBefore_WhenUpcoming`; opt-out respeitado.

### Fase 8.3 — Campanhas e NPS — Concluída (ADR-040)

- Campanhas `Inactive90Days` (launch manual) e `PostAppointment` (scan NPS); `MarketingEnabled` em preferências tutor.
- API campanhas/NPS + público `/api/v1/public/nps/{token}`; UI `/automations` e `/nps/{token}`.
- Aceite: `Campaign_EnqueuedForInactiveSegment_WhenLastVisitOlderThan90Days`; `NpsResponse_RecordedAndReportable`.

### Fase 8.4 — Módulo TutorPortal — base — Concluída (ADR-041)

- `src/Modules/TutorPortal/`, role `Tutor`, `TutorPortalAccount`, API `/api/v1/tutor-portal/`, client `TutorPortalWeb`.
- Aceite: `Tutor_AuthenticatesSeparatelyFromClinicStaff`; isolamento staff/tutor nos testes de integração.

### Fase 8.5 — App do tutor — Concluída (ADR-042)

- API `/api/v1/tutor-portal/pets/{id}/vaccination-card|exams|timeline`; push subscribe + VAPID opcional.
- `TutorPortalWeb`: hub do pet, carteira read-only (SharedUI), exames, timeline; Web Push no SW quando configurado.
- Aceite: `Tutor_ViewsVaccinesAndExams_OfAuthorizedPet`; isolamento staff/tutor.

### Fase 8.6 — Autoagendamento pelo tutor — Concluída (ADR-043)

- API `/api/v1/tutor-portal/pets/{id}/booking/*`; schedulers compartilhados Veterinary/Petshop; `TutorPortalWeb` `/pets/{id}/schedule`.
- Aceite: `Tutor_BooksAppointment_AppearsOnClinicDailySchedule`; isolamento staff/tutor.

### Fase 8.7 — Site do estabelecimento — Concluída (ADR-044)

- Módulo `ClinicSite`, API staff `/api/v1/clinic-site/` e pública `/api/v1/public/clinic-sites/{slug}`; clients `ClinicSiteWeb` + SharedUI `/clinic-site`.
- Aceite: `ClinicSite_PublishedWithCadastralData_WhenSlugResolved`.

### Fase 8.8 — E-commerce e marketplaces — Concluída (ADR-045)

- Módulo `Commerce`, API staff `/api/v1/commerce/` e loja pública por slug; `ClinicSiteWeb` `/loja`; SharedUI `/commerce/offers|orders`.
- Aceite: `EcommerceOrder_DebitsStock_WhenConfirmed`; `OfferPrice_ReflectsBackofficeChange_OnPublicCatalog`; `MercadoLivreInboundOrder_DebitsStock`.

### Fase 9.1 — Módulo Platform (multi-tenancy) — Concluída (ADR-046)

- Módulo `Platform`, `TenantResolutionMiddleware`, `TenantRequiredEndpointFilter`, `TenantSchema.FromId`, filtros EF por tenant nos DbContexts.
- Aceite: `TenantIsolation_DoesNotLeakCrm_WhenDifferentTenants`.

### Fase 9.2 — Gestão de tenants e filiais — Concluída (ADR-047)

- API `/api/v1/platform/tenants` (SuperAdmin): onboarding, status, soft delete, filiais.
- `ITenantProvisioner`, `ITenantSignInGate`, role `SuperAdmin`.
- Aceite: `TenantOnboarding_AdminCanAuthenticate_WhenProvisioned`; `SuspendedTenant_ForbiddenOnClinicApi`; `Branch_LinkedToHeadquarters_WhenAdded`.

### Fase 9.3 — Planos, add-ons e feature flags — Concluída (ADR-048)

- Catálogo Starter/Pro/Hospital24h + add-ons; assinatura, trial, pró-rata e flags por tenant.
- `ITenantEntitlementReader`, filtro `CommercialModuleEndpointFilter`, menus filtrados em `/auth/me`.
- API Super Admin: `/api/v1/platform/plans`, `/addons`, assinatura, flags e entitlements.
- Aceite: `FeatureFlag_DisablesModule_OnApiAndMenus`; `PlanChange_ReturnsProration_WhenMidCycle`.

### Fase 9.4 — Gateway de assinatura e cobrança — Concluída (ADR-049)

- `IBillingGateway` (Fake/Asaas), faturas, webhook idempotente, `BillingCycleHostedService`.
- API: `/api/v1/platform/tenants/{id}/billing/*`, webhook `POST /api/v1/platform/webhooks/asaas`.
- Aceite: `RecurringSubscription_Charged_WhenPeriodDue`; `PaymentWebhook_UpdatesBillingStanding_WhenPaid`.

### Fase 9.5 — Dunning, bloqueio e cupons — Concluída (ADR-050)

- Régua dunning (e-mail/SMS Fake/Live), retentativa cartão, lock operacional após carência.
- API clínica `/api/v1/billing/standing|pay`; cupons Super Admin `/api/v1/platform/coupons`.
- SharedUI `/billing/payment` quando `BillingStanding.Locked`.
- Aceite: `SimulatedDelinquency_SuspendsOperationalAccess`.

### 👉 **Próxima Ação: Fase 9.6 — NFS-e do SaaS e impersonation**

Ver [`roadmap.md`](roadmap.md) §9.6 e [`backoffice.md`](backoffice.md) §1 e §3.

### Fase 3.5 (Motor de sincronização) — Concluída (ADR-002)

- Outbox completo tutor/pet/delete; `PushSyncBatchCommand` + `PullChangesQuery`; LWW; retry/dead-letter; `SyncBackgroundWorker` + `SetSyncing`.

**Nota:** MAUI build local exige workload; Linux CI continua via `SaaS_Veterinario.ci.slnf` (sem MAUI).

---

> **Regra Mestra do Projeto:** O TDD será estritamente seguido (Testes RED -> Implementação GREEN -> REFACTOR) e a sub-entrega B só será considerada concluída quando o build estiver limpo (0 warnings obstrutivos) e 100% dos testes rodarem em verde.
