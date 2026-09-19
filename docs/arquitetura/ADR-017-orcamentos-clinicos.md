# ADR-017: Orçamentos clínicos e fila PDV

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 4.5 exige orçamentos vinculados a atendimento/pet, status rascunho → enviado → aprovado/recusado, e aceite: aprovado gera item pendente para conversão em venda (PDV Fase 6). Sales ainda exige caixa aberto para `Order`; módulos não se referenciam diretamente (ADR-001).

## Opções consideradas
1. **Criar `Order` no approve** — viola caixa e acopla Veterinary → Sales.
2. **Fila pending + evento de integração** — Veterinary persiste `ConversionStatus.Pending`; query inbox; `ClinicalQuoteApprovedEvent` em Core para handler Sales futuro.
3. **Tabela de conversão em Sales** — duplicaria estado; adiado à Fase 6.

## Decisão
- Agregado `ClinicalQuote` / `ClinicalQuoteItem` no Veterinary com snapshot de preço (decimal BRL).
- Aprovação levanta `ClinicalQuoteApprovedDomainEvent` → `ClinicalQuoteApprovedEvent` (MediatR).
- Inbox: `GET /api/v1/clinical-quotes/pending-conversions`.
- Permissões `ClinicalQuotes.Read/Write`; recepção com escrita; caixa só leitura.
- Sync: pull de orçamentos; outbox client para create/replace/send/approve/reject.
- Print via Blazor `@media print` (ADR-016).

## Consequências
- PDV Fase 6.1: `CreateOrder` com `SourceQuoteId`; ao pagar, `ClinicalQuoteConvertedEvent` → `ClinicalQuoteConvertedIntegrationHandler` chama `MarkConverted(orderId)` (ADR-025).
- Inventory sem catálogo de preços: linhas livres com `ProductId` opcional; preço snapshot na venda.

## Referências
- [`docs/diagramas/orcamentos-clinicos.mmd`](../diagramas/orcamentos-clinicos.mmd)
- [`ADR-025`](./ADR-025-motor-pdv-vendas.md)
- [`sync-poc.md`](./sync-poc.md)
