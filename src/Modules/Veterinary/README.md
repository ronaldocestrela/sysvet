# `src/Modules/Veterinary/` — Módulo Clínico Veterinário

Módulo responsável por operações **clínicas**: agenda unificada, prontuário, vacinas e internação.

## Status

> **Em progresso (Fase 4).** Agenda clínica unificada (4.1) implementada; prontuário completo (4.2+) em evolução.

## Escopo de Negócio

| Área | Status |
|------|--------|
| **Agenda** (`Appointment`, `ScheduleSlot`) | Concluído (4.1) |
| **Prontuário** (`MedicalRecord`) | Concluído (4.2 — anamnese, vitais, evolução, timeline, sync) |
| **Vacinas** (`VaccineDose`) | Parcial |
| **Internação** (`Hospitalization`) | Parcial |

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Agregados, enums, repositórios |
| [`Application/`](./Application/) | CQRS MediatR, DTOs, validação |
| [`Infrastructure/`](./Infrastructure/) | `VeterinaryDbContext`, sync plugin (`ISyncPushHandler`, `ISyncChangeFeedContributor`) |

## Dependências

- Referencia `Core.Domain` (IDs de `Tutor`/`Pet` apenas)
- **Não** referencia outros módulos de negócio
