# ADR-030: Estética — banho e tosa (Petshop)

## Status
Accepted

## Data
2026-09-21

## Contexto
A fase 6.6 exige agenda de banhistas/tosadores, ficha digital B&T por pet e baixa de insumos no estoque ao concluir o serviço. O módulo Petshop estava apenas como stub; pacotes pré-pagos e PDV já vivem no Sales (ADR-029). Módulos devem permanecer comercialmente independentes (ADR-001).

## Opções consideradas
1. **Reutilizar agenda Veterinary** — acopla estética ao clínico e impede venda isolada do módulo Petshop.
2. **Petshop com agenda própria + integração Core** — espelha padrão de estoque/pré-pago já usado no PDV.

## Decisão
- Agregados `GroomingAppointment`, `GroomingSlot`, `GroomingRecord`, `GroomingService` no Petshop com `PetshopDbContext`.
- **`ConsumeStockForGroomingRequest`** no Core; handler Inventory com `StockMovementReasons.Grooming` e `CorrelationId = AttendanceId`.
- Conclusão chama **`ConsumePrepaidServicePackageRequest`** quando o serviço tem `PrepaidServiceCode`; `Package.NotFound` e saldo insuficiente não abortam conclusão (banho avulso).
- Permissões `Grooming.Read` / `Grooming.Write`; menu `grooming`; sem nova role Identity.

## Consequências
- Migration Petshop; sync push/pull; SQLite offline espelhando Veterinary/Sales.
- Eventos `GroomingStarted` / `GroomingCompleted` / `GroomingReadyForPickup` — notificações na fase 6.7 (ADR-031).

## Confirmação no código
- [`ConsumeStockForGroomingRequest`](../../src/Modules/Core/Application/IntegrationEvents/ConsumeStockForGroomingRequest.cs)
- [`ConsumeStockForGroomingRequestHandler`](../../src/Modules/Inventory/Application/StockSales/ConsumeStockForGroomingRequestHandler.cs)
- Módulo [`Petshop`](../../src/Modules/Petshop/)

## Relacionados
- [ADR-001](./ADR-001-monolito-modular.md), [ADR-029](./ADR-029-pacotes-kits-prepagos.md)
- [`docs/roadmap.md`](../roadmap.md) § 6.6
