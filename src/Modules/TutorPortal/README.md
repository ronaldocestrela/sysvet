# Módulo TutorPortal

Canal digital do tutor (cliente CRM): autenticação isolada da clínica, vínculo Identity ↔ tutor CRM e API `/api/v1/tutor-portal/`.

## Escopo

### Fase 8.4

- Role Identity `Tutor` e policy `TutorPortal`
- Auto-cadastro por e-mail + CPF (mesmo registro CRM)
- Login, refresh e `GET /me` (pets resumidos)
- Client WASM [`TutorPortalWeb`](../../Clients/TutorPortalWeb/)

### Fase 8.5

- `GET /pets/{petId}/vaccination-card`, `/exams`, `/timeline` (pet do tutor)
- Web Push: `TutorPushSubscription`, `POST /push/subscribe`, VAPID opcional
- Read port `ITutorPetHealthReadPort` (Veterinary + Petshop)

### Fase 8.6

- Autoagendamento: `GET/POST …/pets/{petId}/booking/*` (serviços, profissionais, slots, reserva, cancelamento)
- Port `ITutorSchedulingPort` + schedulers compartilhados (`IAppointmentScheduler`, `IGroomingAppointmentScheduler`)
- PWA `/pets/{petId}/schedule`

## Fora de escopo nesta fatia

Site, e-commerce (roadmap 8.7–8.8); anexos clínicos blob; reagendamento tutor.

## Referências

- [ADR-041](../../../docs/arquitetura/ADR-041-tutor-portal-base.md)
- [ADR-042](../../../docs/arquitetura/ADR-042-tutor-portal-app.md)
- [ADR-043](../../../docs/arquitetura/ADR-043-tutor-autoagendamento.md)
- [roadmap § 8.4–8.6](../../../docs/roadmap.md)
