# ADR-042: TutorPortal — app do tutor (8.5)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.5 exige PWA do tutor com carteira de vacinação, histórico de exames, timeline de atendimentos e push quando disponível. A 8.4 entregou autenticação isolada ([ADR-041](./ADR-041-tutor-portal-base.md)); queries clínicas do Veterinary permanecem restritas a staff (`ClinicStaff`/`Veterinarian`).

## Opções consideradas

1. **Reutilizar endpoints `/api/v1/pets/{id}/…` com role Tutor** — ampliaria RBAC clínico e vazaria superfície staff; rejeitado.
2. **Endpoints dedicados `/api/v1/tutor-portal/pets/{id}/…` + read port cross-module** — aceito; espelha `ITutorVisitReadPort` (ADR-040).
3. **App MAUI do tutor** — fora do escopo imediato; `TutorPortalWeb` PWA fino (ADR-041) estendido.

## Decisão

1. **Read port** `ITutorPetHealthReadPort` (Infrastructure: `VeterinaryDbContext` + `PetshopDbContext`) para carteira, exames e timeline sanitizada (sem anamnese/diagnóstico).
2. **Autorização:** policy `TutorPortal` + `TutorPetAccessGuard` (pet ativo com `TutorId` do JWT); outro tutor → `TutorPortal.Pet.NotFound`.
3. **API:** `GET …/vaccination-card`, `…/exams`, `…/timeline`; push `GET …/push/vapid-public-key`, `POST …/subscribe|unsubscribe`.
4. **Push:** agregado `TutorPushSubscription`; `ITutorPushSender` Fake por padrão, `WebPush` quando `TutorPortal:Vapid*` configurado; service worker com handlers `push` / `notificationclick`.
5. **UI:** `TutorPortalWeb` + componente read-only `VaccinationCardDisplay` (SharedUI).

## Consequências

- Aceite: `Tutor_ViewsVaccinesAndExams_OfAuthorizedPet`; isolamento staff/tutor nos endpoints de pet health.
- Autoagendamento (8.6), site e e-commerce permanecem no roadmap.
- Download de anexos clínicos (ADR-015) não exposto ao tutor nesta fatia.

## Confirmação no código

- [`TutorPortalEndpointExtensions`](../../src/API/Extensions/TutorPortalEndpointExtensions.cs)
- [`TutorPetHealthReadPort`](../../src/Modules/TutorPortal/Infrastructure/PetHealth/TutorPetHealthReadPort.cs)
- [`TutorPortalWeb/Pages`](../../src/Clients/TutorPortalWeb/Pages/)

## Relacionados

- [ADR-016](./ADR-016-carteira-vacinacao.md), [ADR-041](./ADR-041-tutor-portal-base.md)
- [`docs/diagramas/tutor-portal-app.mmd`](../diagramas/tutor-portal-app.mmd)
