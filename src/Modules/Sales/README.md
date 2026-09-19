# `src/Modules/Sales/` — Módulo de Vendas e PDV

Módulo responsável pelo **Ponto de Venda (PDV)** da clínica/petshop. A **Fase 6.1** entrega o motor de vendas **online**; fila offline e sync ficam na 6.2.

## Status

> **Fase 6.1 concluída (ADR-025).** Domínio, Application, Infrastructure, endpoints API e clients SharedUI integrados. TEF, comissões e PDV offline são fases posteriores.

## Escopo atual (6.1)

- **Pedidos:** linhas produto (`ProductId` + baixa de estoque) e serviço (sem estoque); preço snapshot na linha (ADR-019)
- **Pagamentos:** múltiplas formas por pedido (`Cash`, cartões, `Pix`); registro sem TEF (6.3)
- **Caixa:** abertura/fechamento; saldo atual derivado na query (abertura + vendas em dinheiro da sessão)
- **CRM:** tutor/pet opcionais na venda; conversão de orçamento clínico via `SourceQuoteId` + `ClinicalQuoteConvertedEvent`
- **Integração:** `ConsumeStockForSaleRequest` no pay; `OrderPaidEvent` com receita **`FinanceIntegrationStatus.Pending`** (módulo Finance 7.1+)

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | `Order`, `OrderItem`, `Payment`, `CashRegister`; enums de status/kind/método/finance |
| [`Application/`](./Application/) | Commands/queries PDV; validators; handlers de pay com estoque e eventos |
| [`Infrastructure/`](./Infrastructure/) | `SalesDbContext`, migrations, repositórios EF |

## Dependências

- **Core:** tutor/pet repositories, `ConsumeStockForSaleRequest`, eventos de integração
- **Inventory:** handler de baixa de estoque no pay (sem referência Sales → Inventory no Domain)
- **Veterinary:** consumer de `ClinicalQuoteConvertedEvent` para `MarkConverted`
- **Clients:** [`SalesApiService`](../../Clients/Clients.Infrastructure/Http/SalesApiService.cs) — checkout exige rede

## Referências

- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md)
- [`docs/diagramas/pdv-motor-vendas.mmd`](../../../docs/diagramas/pdv-motor-vendas.mmd)
