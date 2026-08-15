# `src/Modules/Sales/` — Módulo de Vendas e PDV

Módulo responsável pelo **Ponto de Venda (PDV)** da clínica/petshop. Suporta operação **offline** — vendas registradas localmente são sincronizadas com o servidor quando a conexão é restaurada.

## Status

> 🔴 **Não iniciado.** As subpastas de camada existem mas estão vazias (apenas `.gitkeep` e arquivos de projeto).

## Escopo de Negócio

Este módulo gerenciará:
- **PDV offline**: criação de pedidos e recebimento de pagamentos sem conexão com internet
- **Pedidos**: registro de produtos/serviços vendidos com descontos e totais
- **Pagamentos**: múltiplas formas de pagamento por pedido (dinheiro, cartão, PIX)
- **Comissões**: cálculo automático de comissão por funcionário com base nas vendas
- **Sincronização**: fila de pedidos pendentes de envio ao servidor (offline-first)
- **Caixa**: abertura, fechamento e sangria de caixa com controle de troco

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades: `Order`, `OrderItem`, `Payment`, `CashRegister`. Value Objects: `Money`, `Discount`. Enums: `PaymentMethod`, `OrderStatus`. |
| [`Application/`](./Application/) | Commands: `CreateOrder`, `AddOrderItem`, `ProcessPayment`, `SyncOfflineOrders`. Queries: `GetOrderById`, `GetSalesReport`. |
| [`Infrastructure/`](./Infrastructure/) | `SalesDbContext` (SQL Server + SQLite), repositórios, serviço de fila de sincronização offline. |

## Dependências

- Referencia `Core.Domain` para `Tutor` (cliente da venda)
- Integra-se ao `Inventory` para baixar estoque ao confirmar uma venda
- Integra-se ao `Fiscal` para emissão de NF após pagamento
