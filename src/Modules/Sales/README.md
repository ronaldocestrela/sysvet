# `src/Modules/Sales/` — Módulo de Vendas e PDV

Módulo responsável pelo **Ponto de Venda (PDV)** da clínica/petshop. **6.1–6.3** entregam motor online, checkout offline-first e pagamentos eletrônicos com TEF (PoC).

## Status

> **Fases 6.1 (ADR-025), 6.2 (ADR-026) e 6.3 (ADR-027) concluídas.** Domínio, Application, sync plugin, API e clients (`ISalesStore`) integrados. Comissões e devoluções de venda são fases posteriores (6.4).

## Escopo

- **Pedidos:** linhas produto (`ProductId` + baixa de estoque) e serviço (sem estoque); preço snapshot na linha (ADR-019)
- **Pagamentos:** split (`Cash`, cartões, `Pix`); **NSU** e metadados TEF; estorno parcial/total (`PaymentRefund`) sem baixa de estoque
- **TEF:** `IPaymentTerminal` + `SimulatedPaymentTerminal` (offline-capable); adapters reais plugáveis
- **Caixa:** abertura/fechamento; saldo gaveta = abertura + líquido em dinheiro; totais por forma (bruto/estornado/líquido)
- **CRM:** tutor/pet opcionais; conversão de orçamento clínico (`ClinicalQuoteConvertedEvent`)
- **Integração:** `ConsumeStockForSaleRequest` no pay; `OrderPaidEvent` / `OrderPaymentRefundedEvent`; `FinanceIntegrationStatus.Pending`
- **Offline (6.2–6.3):** outbox Create+Pay+Refund; autorização TEF local; replay com NSU não reautoriza

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | `Order`, `Payment`, `PaymentRefund`, `CashRegister`; `IPaymentTerminal` |
| [`Application/`](./Application/) | CQRS PDV, pay/refund, caixa, validators |
| [`Infrastructure/`](./Infrastructure/) | EF, migrations, sync, DI do simulador |

## Dependências

- **Core:** tutor/pet, `ConsumeStockForSaleRequest`, eventos, outbox sync
- **Inventory:** baixa de estoque no pay
- **Veterinary:** `ClinicalQuoteConvertedEvent`
- **Clients:** [`ISalesStore`](../../Clients/Clients.Infrastructure/Sales/ISalesStore.cs), POS/caixa/comprovante SharedUI

## Referências

- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md)
- [`docs/arquitetura/ADR-026-pdv-offline-sync.md`](../../../docs/arquitetura/ADR-026-pdv-offline-sync.md)
- [`docs/arquitetura/ADR-027-pagamentos-tef.md`](../../../docs/arquitetura/ADR-027-pagamentos-tef.md)
- [`docs/diagramas/pdv-pagamentos-tef.mmd`](../../../docs/diagramas/pdv-pagamentos-tef.mmd)
