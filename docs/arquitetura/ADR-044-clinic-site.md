# ADR-044: Site do estabelecimento — Fase 8.7

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.7 exige site público por subdomínio `{slug}.vetnexus.app` com páginas de serviços, equipe, contato e horários, alimentado pelos dados cadastrais da clínica. TutorPortal (ADR-041–043) cobre canal autenticado do tutor; Fiscal `IssuerProfile` é emitente NF-e, não vitrine marketing.

## Opções consideradas

1. **Reutilizar BlazorWeb staff com rotas públicas** — risco de vazar backoffice; rejeitado.
2. **Live-pull de GroomingService/AppUser/AutomationsSettings** — vazamento de catálogo interno e acoplamento; rejeitado.
3. **Módulo `ClinicSite` + PWA anônimo + API `/api/v1/public/clinic-sites/{slug}`** — aceito; espelha NPS público (ADR-040) e TutorPortal (superfície dedicada).

## Decisão

1. Módulo **`ClinicSite`** (Domain/Application/Infrastructure) com agregado `ClinicSiteProfile` (singleton por tenant) e filhos (serviços, equipe, horários).
2. **Índice global** `ClinicSiteSlugIndex` em schema `dbo` (`slug → TenantId`, `IsPublished`); conteúdo no schema do tenant (ADR-003).
3. Staff: `GET/PUT /api/v1/clinic-site`, `PUT …/services|team|hours`, `POST …/publish|unpublish` com `ClinicSite.Read` / `ClinicSite.Write`.
4. Público: `GET /api/v1/public/clinic-sites/{slug}` + `ClinicSitePublicTenantFilter` (tenant antes do handler).
5. Client **`ClinicSiteWeb`** (WASM PWA, sem auth); slug via subdomínio ou rota `/s/{slug}` em dev.
6. Backoffice: página SharedUI `/clinic-site` + `ClinicSiteApiService`.

## Consequências

- Aceite: `ClinicSite_PublishedWithCadastralData_WhenSlugResolved`; `ClinicSite_NotFound_WhenUnpublishedOrUnknownSlug`; `ClinicStaffToken_RequiredToPublishClinicSite`.
- DNS wildcard `*.vetnexus.app` e módulo Platform (9.x) ficam fora desta fatia.
- Logo via URL opcional; upload blob fica para evolução.

## Confirmação no código

- [`ClinicSiteEndpointExtensions`](../../src/API/Extensions/ClinicSiteEndpointExtensions.cs)
- [`ClinicSitePublicTenantFilter`](../../src/API/Filters/ClinicSitePublicTenantFilter.cs)
- [`ClinicSiteWeb/Program.cs`](../../src/Clients/ClinicSiteWeb/Program.cs)
- [`SharedUI/Pages/ClinicSite.razor`](../../src/Clients/SharedUI/Pages/ClinicSite.razor)

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-040](./ADR-040-campanhas-nps.md), [ADR-041](./ADR-041-tutor-portal-base.md)
- [`docs/diagramas/clinic-site.mmd`](../diagramas/clinic-site.mmd)
