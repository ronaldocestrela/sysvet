# `src/Modules/Sales/` — Módulo de Vendas e PDV

Módulo responsável pelo **Ponto de Venda (PDV)** da clínica/petshop. **6.1–6.4** entregam motor online, checkout offline-first, TEF e comissões/descontos/devoluções.

## Status

> **Fases 6.1–6.5 concluídas (ADR-025–029).** Domínio, Application, sync, API e clients (`ISalesStore`) integrados.

## Escopo

- **Pedidos:** linhas produto/serviço; desconto percentual com teto de perfil; vendedor e performer opcional por linha
- **Pagamentos:** split + NSU/TEF; estorno de pagamento (`PaymentRefund`) sem estoque
- **Devoluções:** `ReturnItems` → `SaleReturn`, estoque `SaleReturn`, estorno caixa proporcional, reversão de comissão
- **Comissões:** `CommissionRule` + `CommissionCalculator` → `CommissionAccrual` no pay
- **Caixa:** abertura/fechamento; saldo gaveta e totais por forma
- **Offline:** outbox Create/Pay/Refund/Return/Consume; regras de comissão via pull; comissão calculada localmente no pay
- **Pacotes/kits:** `ProductKit`, `ServicePackage`, saldo `PrepaidBalance`; consumo via API ou request Core (6.6 Petshop)

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | `Order`, `Payment`, `CommissionRule`, `SaleReturn`, … |
| [`Application/`](./Application/) | CQRS PDV, comissões, devoluções |
| [`Infrastructure/`](./Infrastructure/) | EF, migrations, sync |

## Referências

- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md)
- [`docs/arquitetura/ADR-026-pdv-offline-sync.md`](../../../docs/arquitetura/ADR-026-pdv-offline-sync.md)
- [`docs/arquitetura/ADR-027-pagamentos-tef.md`](../../../docs/arquitetura/ADR-027-pagamentos-tef.md)
- [`docs/arquitetura/ADR-028-comissoes-descontos-devolucoes-venda.md`](../../../docs/arquitetura/ADR-028-comissoes-descontos-devolucoes-venda.md)
- [`docs/arquitetura/ADR-029-pacotes-kits-prepagos.md`](../../../docs/arquitetura/ADR-029-pacotes-kits-prepagos.md)
- [`docs/diagramas/pdv-comissoes-devolucoes.mmd`](../../../docs/diagramas/pdv-comissoes-devolucoes.mmd)
