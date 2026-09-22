# ADR-041: TutorPortal — autenticação isolada e auto-cadastro (8.4)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.4 exige canal digital separado para o tutor (cliente CRM), com role Identity `Tutor` e API dedicada, sem expor o backoffice clínico. ADR-007 havia deixado a role `Tutor` fora do escopo 2.3; ADR-012 mantém JWT direto no WASM (sem BFF).

## Opções consideradas

1. **Área isolada no BlazorWeb staff** — risco de vazar rotas SharedUI; rejeitado.
2. **Client WASM dedicado + endpoints `/api/v1/tutor-portal/`** — aceito; alinhado ao monólito modular (ADR-001).
3. **`TutorId` em `AppUser`** — acopla Identity ao CRM; rejeitado em favor de agregado `TutorPortalAccount` no módulo TutorPortal.

## Decisão

1. Módulo **`TutorPortal`** (Domain/Application/Infrastructure) com `TutorPortalAccount` (`UserId`, `TutorId`) e `TutorPortalDbContext`.
2. Role **`Tutor`** seedada via `ApplicationRoles.AllIncludingTutor`; policies **`TutorPortal`** (só Tutor) e **`ClinicUser`** (staff, sem Tutor).
3. **Superfícies de login distintas:** `POST /api/v1/auth/login` recusa role Tutor; `POST /api/v1/tutor-portal/login` recusa staff; código `Auth.WrongPortal` / `TutorPortal.WrongPortal` → HTTP 401.
4. **Auto-cadastro:** `POST /api/v1/tutor-portal/register` exige e-mail **e** CPF iguais ao mesmo tutor CRM ativo; falhas genéricas `TutorPortal.RegistrationDenied`.
5. JWT reutiliza emissor Core; claim opcional **`TutorId`** quando há vínculo portal.
6. Client **`TutorPortalWeb`** (PWA fino, sem SQLite/sync); tokens em `localStorage` com prefixo `tutorportal_*`.

## Consequências

- Aceite: `Tutor_AuthenticatesSeparatelyFromClinicStaff`; `TutorToken_ForbiddenOnClinicTutorsApi`; `ClinicStaffToken_ForbiddenOnTutorPortalMe`; `TutorSelfRegister_LinksExistingCrmTutor_WhenEmailAndCpfMatch`.
- Vacinas, exames, agenda e e-commerce permanecem nas fases 8.5–8.8.

## Confirmação no código

- [`TutorPortalEndpointExtensions`](../../src/API/Extensions/TutorPortalEndpointExtensions.cs)
- [`RegisterTutorCommandHandler`](../../src/Modules/TutorPortal/Application/Auth/Commands/RegisterTutorCommandHandler.cs)
- [`TutorPortalWeb/Program.cs`](../../src/Clients/TutorPortalWeb/Program.cs)

## Relacionados

- [ADR-007](./ADR-007-jwt-rbac.md), [ADR-012](./ADR-012-blazor-pwa-jwt.md), [ADR-001](./ADR-001-monolito-modular.md)
- [`docs/diagramas/tutor-portal-auth.mmd`](../diagramas/tutor-portal-auth.mmd)
