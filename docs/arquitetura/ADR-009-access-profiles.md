# ADR 009: Perfis de acesso customizáveis (híbrido com RBAC Identity)

## Status
Accepted

## Data
2026-09-16

## Contexto

A Fase 2.5 exige CRUD de usuários por tenant, perfis customizáveis (matriz permissão × recurso) e preferências de UI, com aceite de admin alterando permissões e usuário vendo apenas menus permitidos. A Fase 2.3 (ADR-007) já define JWT, refresh tokens e RBAC fixo por roles Identity (`Admin`, `Veterinarian`, `Receptionist`, `Cashier`) com policies ASP.NET.

**Drivers:** granularidade além de `RequireRole`; efeito imediato de revogação sem reemitir JWT gigante; manter compatibilidade com handlers e policies existentes.

## Opções consideradas

1. **Substituir roles por matriz única** — policies resolvem só permissões.
   - Contras: refatoração massiva de 2.3; quebra mental model atual.

2. **Híbrido: policy + permission code** — roles/policies permanecem gate grosso; `AccessProfile` por tenant define matriz fina; `AuthorizationBehavior` exige ambos quando permission é declarada.
   - Prós: evolução incremental; Cashier continua fora de `ClinicStaff`; prova fina no CRM.

3. **Permissões no JWT**
   - Contras: tokens grandes; alteração de matriz não reflete até refresh.

## Decisão

1. **Catálogo de permissões** em código (`Permissions.*`); admin edita apenas combinações válidas no perfil.
2. **Agregado `AccessProfile`** no Domain; **`AppUser`** permanece na Infrastructure (ADR-001).
3. **Permissões não entram no JWT**; resolução por request via `AccessProfileId` em `AppUser` + `IPermissionChecker`.
4. **Quatro perfis sistema** (`IsSystem=true`), um por `ApplicationRoles`, seed idempotente por tenant: nome/`BaseRole` imutáveis, matriz editável, sem delete.
5. **Perfis custom:** criados por clone; delete apenas sem usuários vinculados.
6. **`BaseRole` do perfil** sincroniza a role Identity do usuário (`ReplaceRole`).
7. **`GET /api/v1/auth/me`** expõe `Permissions` e `Menus` derivados do perfil (contrato para Fase 3.1).
8. **Preferências** (`UserPreference`): JSON versionado no servidor, escopo usuário autenticado.

## Consequências

- **Positivas:** aceite 2.5 sem reescrever ADR-007; enforcement fino demonstrável em CRM write/delete.
- **Negativas:** lookup extra por request; módulos satélites ainda só policy até sprints futuras.
- **Futuro:** auditoria 2.6 para alterações de matriz; claim opcional `profile_id` no JWT.

## Confirmação no código

- Domain: `AccessProfile`, `PermissionCode`, `Permissions`, `MenuCatalog`, `UserPreference`.
- Application: `Users/`, `AccessProfiles/`, `Preferences/`; `AuthorizeRequestAttribute` com permission opcional.
- Infrastructure: migration `AddAccessProfilesAndUserPreferences`, `AccessProfileSeeder`, `PermissionChecker`.
- API: `UserEndpointsExtensions`, `AccessProfileEndpointsExtensions`, `PreferenceEndpointsExtensions`.

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-004](./ADR-004-padrao-cqrs.md), [ADR-007](./ADR-007-jwt-rbac.md)
- [roadmap](../roadmap.md) — item 2.5
