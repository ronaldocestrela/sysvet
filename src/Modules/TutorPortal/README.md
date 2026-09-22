# Módulo TutorPortal

Canal digital do tutor (cliente CRM): autenticação isolada da clínica, vínculo Identity ↔ tutor CRM e API `/api/v1/tutor-portal/`.

## Escopo (Fase 8.4)

- Role Identity `Tutor` e policy `TutorPortal`
- Auto-cadastro por e-mail + CPF (mesmo registro CRM)
- Login, refresh e `GET /me` (pets resumidos)
- Client WASM [`TutorPortalWeb`](../../Clients/TutorPortalWeb/)

## Fora de escopo nesta fatia

Carteira de vacinas, exames, timeline, autoagendamento, site e e-commerce (roadmap 8.5–8.8).

## Referências

- [ADR-041](../../../docs/arquitetura/ADR-041-tutor-portal-base.md)
- [roadmap § 8.4](../../../docs/roadmap.md)
