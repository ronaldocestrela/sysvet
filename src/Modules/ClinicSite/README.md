# Módulo ClinicSite

Site público do estabelecimento (Fase 8.7): vitrine anônima por slug/subdomínio e edição no backoffice.

## Escopo

- Agregado `ClinicSiteProfile` + serviços, equipe e horários
- Índice global `ClinicSiteSlugIndex` (`dbo.ClinicSiteSlugs`)
- API staff `/api/v1/clinic-site/*`
- API pública `/api/v1/public/clinic-sites/{slug}`
- Client WASM [`ClinicSiteWeb`](../../Clients/ClinicSiteWeb/)
- UI staff [`SharedUI/Pages/ClinicSite.razor`](../../Clients/SharedUI/Pages/ClinicSite.razor)

## Fora de escopo

E-commerce (8.8), domínio customizado, CMS, upload de logo (apenas URL).

## Referências

- [ADR-044](../../../docs/arquitetura/ADR-044-clinic-site.md)
- [roadmap § 8.7](../../../docs/roadmap.md)
