# `src/Modules/Inventory/` — Módulo de Estoque

Módulo responsável pelo **controle de estoque** de produtos comercializados e insumos utilizados nos serviços veterinários e de petshop.

## Status

> **Fase 5.1 concluída:** catálogo de produtos, fornecedores, lotes com validade/custo e saldo por lote. Movimentações avançadas, alertas e compras permanecem nas tarefas 5.2+.

## Escopo de Negócio

- **Cadastro de produtos**: SKU, código de barras, categoria, fornecedor, NCM/CEST, custo médio ponderado
- **Lotes**: validade, custo unitário, quantidade on-hand por lote
- **Fornecedores**: CNPJ, contato, vínculo opcional no produto
- **Movimentações** (parcial): entrada/saída legacy + opening balance ao criar lote
- **Saldo**: projeção por produto (`ProductBalance`) derivada dos lotes ativos

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | `Product`, `ProductLot`, `Supplier`, `StockMovement`, `ProductBalance`; VOs; `InventoryCostCalculator` |
| [`Application/`](./Application/) | CQRS produtos/fornecedores/lotes; integração `OrderPaidEvent` |
| [`Infrastructure/`](./Infrastructure/) | `InventoryDbContext`, repositórios, sync plugin (`InventorySync*`) |

## Dependências

- Integra-se ao `Sales` (baixa via `OrderPaidEvent`; evolução lot-aware na 5.2)
- Integra-se ao `Fiscal` (dados fiscais básicos no produto para NF-e futura)

## Referências

- [ADR-019](../../../docs/arquitetura/ADR-019-produtos-lotes-estoque.md)
