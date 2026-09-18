# ADR-018: Internação e mapa de execução

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 4.6 exige leitos/recintos, prescrições horárias materializadas, mapa do dia por leito, administração de doses, evolução e procedimentos internados, com sync offline (ADR-002) e UI SharedUI offline-first.

## Opções consideradas
1. **Reutilizar `IssuedPrescription` e `PrescriptionExecution`** — acopla ambulatorial a internação; execução livre sem slots.
2. **Agregado `Hospitalization` com ordens + slots materializados** — espelha `VaccineSchedule`/`MedicationSchedule`; mapa por `DateOnly` UTC.
3. **Entidade separada `InpatientPrescription`** — duplicaria regras já no agregado de internação.

## Decisão
- Catálogo `WardUnit`/`Bed` (pull-only no client); internação clínica no agregado `Hospitalization` com `BedId`.
- `HospitalMedicationOrder` + `MedicationAdministration` (slots); substituir POST `prescriptions/execute`.
- CQRS + permissões `Hospitalizations.Read/Write`; recepção com leitura do mapa.
- Sync: pull de recintos e internações; outbox para admit/discharge/transfer/ordem/administrar/skip/nota/procedimento.
- UI: mapa por leito (`IHospitalizationStore`); configuração de recintos via API online (`IWardUnitApiService`).

## Consequências
- Estoque (Fase 5) poderá consumir administrações para baixa futura.
- Repositório EF marca filhos novos como `Added` explicitamente ao atualizar agregado rastreado.

## Referências
- [`docs/diagramas/internacao-mapa-execucao.mmd`](../diagramas/internacao-mapa-execucao.mmd)
- [`sync-poc.md`](./sync-poc.md)
