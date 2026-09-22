# ADR-043: TutorPortal — autoagendamento (8.6)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.6 exige que o tutor agende consultas clínicas e serviços de estética pelo portal, com slots por serviço/profissional, confirmação na UI e cancelamento, aparecendo na agenda staff ([roadmap § 8.6](../roadmap.md)). As fases 8.4–8.5 entregaram auth isolada e leitura clínica ([ADR-041](./ADR-041-tutor-portal-base.md), [ADR-042](./ADR-042-tutor-portal-app.md)). A agenda clínica e B&T já existem nos módulos Veterinary e Petshop (4.1 / 6.6).

## Opções consideradas

1. **Reutilizar `/api/v1/appointments` com role Tutor** — ampliaria RBAC staff; rejeitado (ADR-042).
2. **Endpoints `/api/v1/tutor-portal/.../booking` + port cross-module + schedulers compartilhados** — aceito; espelha `ITutorPetHealthReadPort`.
3. **Duplicar regras de slot no TutorPortal** — rejeitado; risco de divergência.

## Decisão

1. **Schedulers reutilizáveis:** `IAppointmentScheduler` (Veterinary) e `IGroomingAppointmentScheduler` (Petshop), usados pelos handlers staff e pela porta do tutor.
2. **Porta:** `ITutorSchedulingPort` em TutorPortal.Application; implementação em Infrastructure (`VeterinaryDbContext`, `PetshopDbContext`, schedulers, `IIdentityService` para nomes).
3. **Serviço clínico virtual:** `Consulta clínica` (30 min, sentinel `ClinicalConsultationServiceId`); estética usa `GroomingService` ativos.
4. **API tutor:** `GET/POST …/pets/{petId}/booking/services|professionals|slots|appointments` e `POST …/cancel`; policy `TutorPortal` + `TutorPetAccessGuard`.
5. **Status:** criação em `Scheduled`; confirmação de domínio permanece staff; cancelamento tutor em `Scheduled`/`Confirmed`.
6. **UI:** `TutorPortalWeb` rota `/pets/{petId}/schedule`; `TutorPortalApiService` estendido.

## Consequências

- Aceite: `Tutor_BooksAppointment_AppearsOnClinicDailySchedule`; booking visível em `GET /api/v1/appointments/daily`.
- Sem catálogo clínico, reagendamento tutor ou pagamento nesta fatia.
- Erros de booking mapeados para `TutorPortal.Booking.*`.

## Confirmação no código

- [`TutorPortalEndpointExtensions`](../../src/API/Extensions/TutorPortalEndpointExtensions.cs) — rotas `/booking`
- [`TutorSchedulingPort`](../../src/Modules/TutorPortal/Infrastructure/Scheduling/TutorSchedulingPort.cs)
- [`AppointmentScheduler`](../../src/Modules/Veterinary/Application/Appointments/AppointmentScheduler.cs)
- [`GroomingAppointmentScheduler`](../../src/Modules/Petshop/Application/GroomingAppointments/GroomingAppointmentScheduler.cs)
- [`PetSchedulePage.razor`](../../src/Clients/TutorPortalWeb/Pages/PetSchedulePage.razor)

## Relacionados

- [ADR-030](./ADR-030-estetica-banho-tosa.md), [ADR-042](./ADR-042-tutor-portal-app.md)
- [`docs/diagramas/tutor-portal-scheduling.mmd`](../diagramas/tutor-portal-scheduling.mmd)
