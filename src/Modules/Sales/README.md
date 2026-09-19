# `src/Modules/Sales/` — Módulo de Vendas e PDV

Módulo responsável pelo **Ponto de Venda (PDV)** da clínica/petshop. **6.1** entrega o motor online; **6.2** adiciona checkout offline-first com outbox e sync (ADR-026).

## Status

> **Fases 6.1 (ADR-025) e 6.2 (ADR-026) concluídas.** Domínio, Application, sync plugin, API e clients (`ISalesStore`) integrados. TEF e comissões são fases posteriores.

## Escopo

- **Pedidos:** linhas produto (`ProductId` + baixa de estoque) e serviço (sem estoque); preço snapshot na linha (ADR-019)
- **Pagamentos:** múltiplas formas por pedido (`Cash`, cartões, `Pix`); registro sem TEF (6.3)
- **Caixa:** abertura/fechamento; saldo atual derivado na query (abertura + vendas em dinheiro da sessão)
- **CRM:** tutor/pet opcionais na venda; conversão de orçamento clínico via `SourceQuoteId` + `ClinicalQuoteConvertedEvent`
- **Integração:** `ConsumeStockForSaleRequest` no pay; `OrderPaidEvent` com receita **`FinanceIntegrationStatus.Pending`** (módulo Finance 7.1+)
- **Offline (6.2):** UUID de pedido/caixa no cliente; push `Open` → `CreateOrder` → `PayOrder`; conflito permanente se estoque recusado no servidor; débito local otimista sem movimento SQLite

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | `Order`, `OrderItem`, `Payment`, `CashRegister`; enums de status/kind/método/finance |
| [`Application/`](./Application/) | Commands/queries PDV; validators; handlers de pay com estoque e eventos |
| [`Infrastructure/`](./Infrastructure/) | `SalesDbContext`, migrations, repositórios EF, [`Sync/`](./Infrastructure/Sync/) push/pull |

## Dependências

- **Core:** tutor/pet repositories, `ConsumeStockForSaleRequest`, eventos de integração, outbox sync
- **Inventory:** handler de baixa de estoque no pay (sem referência Sales → Inventory no Domain)
- **Veterinary:** consumer de `ClinicalQuoteConvertedEvent` para `MarkConverted`
- **Clients:** [`ISalesStore`](../../Clients/Clients.Infrastructure/Sales/ISalesStore.cs) / [`OfflineSalesStore`](../../Clients/Clients.Infrastructure/Sales/OfflineSalesStore.cs); POS/caixa/comprovante SharedUI

## Referências

- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md)
- [`docs/arquitetura/ADR-026-pdv-offline-sync.md`](../../../docs/arquitetura/ADR-026-pdv-offline-sync.md)
- [`docs/diagramas/pdv-motor-vendas.mmd`](../../../docs/diagramas/pdv-motor-vendas.mmd)
- [`docs/diagramas/pdv-offline-sync.mmd`](../../../docs/diagramas/pdv-offline-sync.mmd)
