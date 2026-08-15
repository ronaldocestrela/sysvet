# `src/Modules/Inventory/` — Módulo de Estoque

Módulo responsável pelo **controle de estoque** de produtos comercializados e insumos utilizados nos serviços veterinários e de petshop (medicamentos, rações, acessórios, produtos de banho, etc.).

## Status

> 🔴 **Não iniciado.** As subpastas de camada existem mas estão vazias (apenas `.gitkeep` e arquivos de projeto).

## Escopo de Negócio

Este módulo gerenciará:
- **Cadastro de produtos**: nome, SKU, código de barras, categoria, fornecedor, preço de custo e venda
- **Movimentações**: entrada (compra/devolução) e saída (venda/uso interno) com rastreabilidade
- **Saldo em estoque**: quantidade atual por produto e por unidade/filial
- **Alertas de estoque mínimo**: notificação quando o saldo atingir o ponto de pedido
- **Lotes e validade**: controle de validade para medicamentos e produtos perecíveis
- **Inventário**: contagem física e ajuste de estoque

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades: `Product`, `StockMovement`, `Supplier`, `ProductLot`. Value Objects: `Sku`, `Barcode`, `Money`. Enums: `MovementType`, `ProductCategory`. |
| [`Application/`](./Application/) | Commands: `RegisterProduct`, `RegisterStockEntry`, `DeductStock`. Queries: `GetStockBalance`, `GetLowStockAlerts`, `GetProductById`. |
| [`Infrastructure/`](./Infrastructure/) | `InventoryDbContext`, repositórios, integração com leitores de código de barras (futuro). |

## Dependências

- Integra-se ao `Sales` (baixa de estoque automática ao confirmar venda)
- Integra-se ao `Fiscal` (dados do produto para emissão de NF)
