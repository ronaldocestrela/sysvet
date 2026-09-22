# ADR-016: Carteira de vacinação e alertas

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 4.4 exige protocolos tenant-scoped por espécie/idade, registro de doses com próxima prevista, listagem de alertas no backoffice (SharedUI) e carteira exportável, sem disparo WhatsApp/SMS (Fase 8). Deve reutilizar `VaccineDose` existente e ADR-002 para sync.

## Opções consideradas
1. **PDF server-side (QuestPDF ou similar)** — artefato persistido; mais infra e fora do escopo 8 SP.
2. **Carteira como DTO + página Blazor com `@media print`** — export via imprimir/salvar PDF no browser; zero biblioteca nova.
3. **Alertas via worker + fila** — necessário para Automações 8.x; adiado; query de leitura como contrato estável.

## Decisão
- **Protocolos** no catálogo clínico (`VaccineProtocol` / `VaccineProtocolDose`), filtro por `PetSpecies` e faixa etária quando `Pet.BirthDate` existe.
- **Próxima dose** informada ou calculada por `NextDoseIntervalInDays`; registro livre (nome/lote) continua permitido.
- **Alertas** como projeção CQRS (`Overdue` / `Upcoming`, horizon default 7 dias); disparo automático na 8.2 via `ReminderScheduler` ([ADR-039](./ADR-039-lembretes-smtp-evolution.md)).
- **Exportação** via `GetVaccinationCard` + CSS print na SharedUI.
- **Sync:** protocolos pull-only; doses com outbox no client (espelho templates 4.3).

## Consequências
- SQLite (dev/test): comparações/ordenação de `DateTimeOffset` em repositório de doses feitas em memória após filtro (limitação do provider).
- Recepção ganha `Vaccines.Read` para carteira e alertas; escrita permanece veterinário.

## Referências
- [`docs/diagramas/carteira-vacinacao.mmd`](../diagramas/carteira-vacinacao.mmd)
- [`sync-poc.md`](./sync-poc.md) — escopo 4.4 no pull/push
