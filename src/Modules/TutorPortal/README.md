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

## Fora de escopo nesta fatia

Autoagendamento (8.6), site, e-commerce (roadmap 8.6–8.8); anexos clínicos blob.

## Referências

- [ADR-041](../../../docs/arquitetura/ADR-041-tutor-portal-base.md)
- [ADR-042](../../../docs/arquitetura/ADR-042-tutor-portal-app.md)
- [roadmap § 8.4–8.5](../../../docs/roadmap.md)
