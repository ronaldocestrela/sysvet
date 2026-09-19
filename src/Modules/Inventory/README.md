# `src/Modules/Inventory/` — Módulo de Estoque

Módulo responsável pelo **controle de estoque** de produtos comercializados e insumos utilizados nos serviços veterinários e de petshop.

## Status

> **Fase 5.4 concluída:** perdas auditáveis, fracionamento (`UnitsPerPackage` + lote `-F`), devolução ao fornecedor com evento de integração; além de 5.3 (NF-e compra) e 5.2 (movimentações lot-aware).

## Escopo de Negócio

- **Cadastro de produtos**: SKU, código de barras, categoria, fornecedor, NCM/CEST, custo médio ponderado
- **Lotes**: validade, custo unitário, quantidade on-hand por lote
- **Fornecedores**: CNPJ, contato, vínculo opcional no produto
- **Movimentações**: entrada, saída, ajuste (direção explícita), transferência entre lotes
- **Alertas**: estoque abaixo de `ReorderLevel`, validade próxima/vencida (horizonte configurável)
- **Kardex**: histórico imutável por produto com saldo corrido
- **Saldo**: projeção por produto (`ProductBalance`) derivada dos lotes ativos
- **Entrada NF-e**: upload XML, rascunho, mapeamento assistido de fornecedor/produto, confirmação com movimentações `In`/`Purchase`
- **Perdas**: motivos validade, avaria, consumo interno, doação (`Out` + códigos auditáveis)
- **Fracionamento**: abertura de embalagens para lote fracionado no mesmo SKU
- **Devolução**: saída vinculada ao fornecedor + `SupplierReturnRegisteredEvent` (Finance futuro)

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades, VOs, `StockQuantityApplier`, `LotAllocationService`, `StockAlertClassifier` |
| [`Application/`](./Application/) | CQRS catálogo, movimentações, alertas, integração `OrderPaidEvent` |
| [`Infrastructure/`](./Infrastructure/) | `InventoryDbContext`, repositórios, sync plugin (`InventorySync*`) |

## Dependências

- Integra-se ao `Sales` (baixa FEFO via `OrderPaidEvent`)
- Integra-se ao `Fiscal` (dados fiscais básicos no produto para NF-e futura)

## Referências

- [ADR-019](../../../docs/arquitetura/ADR-019-produtos-lotes-estoque.md)
- [ADR-020](../../../docs/arquitetura/ADR-020-movimentacoes-estoque-alertas.md)
- [ADR-021](../../../docs/arquitetura/ADR-021-entrada-xml-nfe-compra.md)
- [ADR-022](../../../docs/arquitetura/ADR-022-perdas-fracionamento-devolucoes.md)
