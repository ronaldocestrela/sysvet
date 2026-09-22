# Módulo Platform

Catálogo global de tenants (`PlatformTenants`) e portas de resolução (`ITenantSlugLookup`, `ITenantDirectory`) para a Fase 9 do VetNexus.

- **Domain:** `Tenant`, `TenantSlug`, repositório.
- **Application:** contratos de resolução e diretório para workers.
- **Infrastructure:** `PlatformDbContext`, seed do tenant `dev` alinhado a `DevelopmentAdminUserSeeder.DevTenantId`.

Resolução HTTP: [`TenantResolutionMiddleware`](../../API/Middlewares/TenantResolutionMiddleware.cs) (ADR-046).
