# `src/Modules/Petshop/` — Módulo de Estética e Petshop

Módulo responsável pelos serviços de **estética animal**: banho, tosa, hidratação e operação do salão.

## Status

Implementado (fase 6.6): agenda de groomers, ficha digital B&T por pet, baixa de insumos no estoque e integração com pacotes pré-pagos (ADR-029 via Core).

## Escopo de Negócio

- **Agendamentos**: `GroomingAppointment` + `GroomingSlot` (banhistas/tosadores)
- **Catálogo operacional**: `GroomingService` com receita padrão de insumos
- **Ficha digital**: `GroomingRecord` 1:1 com o agendamento, histórico por pet
- **Estoque**: `ConsumeStockForGroomingRequest` (Core → Inventory) na conclusão
- **Pré-pago**: `ConsumePrepaidServicePackageRequest` quando o serviço tem `PrepaidServiceCode`

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Agregados, enums, repositórios, eventos de domínio |
| [`Application/`](./Application/) | CQRS: agenda, slots, ficha, catálogo |
| [`Infrastructure/`](./Infrastructure/) | `PetshopDbContext`, repositórios, sync, DI |

## Dependências

- Referencia `Core.Domain` / `Core.Application` (CRM por Id, contratos de integração)
- **Não** referencia Sales, Inventory ou Veterinary diretamente (ADR-001, ADR-030)

## API (Scalar)

- `/api/v1/grooming-appointments`
- `/api/v1/grooming-slots`
- `/api/v1/grooming-services`
- `/api/v1/pets/{petId}/grooming-history`

## Referências

- [ADR-030](../../docs/arquitetura/ADR-030-estetica-banho-tosa.md)
- [Roadmap 6.6](../../docs/roadmap.md)
