# `src/Modules/Veterinary/` — Módulo Clínico Veterinário

Módulo responsável por operações **clínicas**: agenda unificada, prontuário, vacinas e internação.

## Status

> **Fase 4 clínica concluída (4.1–4.6).** Próximo macro-escopo: Fase 5 (estoque).

## Escopo de Negócio

| Área | Status |
|------|--------|
| **Agenda** (`Appointment`, `ScheduleSlot`) | Concluído (4.1) |
| **Prontuário** (`MedicalRecord`) | Concluído (4.2 — anamnese, vitais, evolução, timeline, sync) |
| **Vacinas** (`VaccineProtocol`, `VaccineDose`, alertas) | Concluído (4.4) |
| **Orçamentos** (`ClinicalQuote`, pending PDV) | Concluído (4.5) |
| **Internação** (`Hospitalization`, mapa de execução, `WardUnit`) | Concluído (4.6) |

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Agregados, enums, repositórios |
| [`Application/`](./Application/) | CQRS MediatR, DTOs, validação |
| [`Infrastructure/`](./Infrastructure/) | `VeterinaryDbContext`, sync plugin (`ISyncPushHandler`, `ISyncChangeFeedContributor`) |

## Dependências

- Referencia `Core.Domain` (IDs de `Tutor`/`Pet` apenas)
- **Não** referencia outros módulos de negócio
