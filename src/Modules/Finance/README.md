# `src/Modules/Finance/` — Módulo Financeiro

Módulo responsável pela **gestão financeira** da clínica/petshop: contas a pagar e receber, caixa, conciliação e fluxo de caixa ([`functions.md`](../../../docs/functions.md) § Financeiro).

## Status

> **Fase 7.1 concluída.** Camadas Domain/Application/Infrastructure, `FinanceDbContext`, DI na API e migration `InitialFinance`. Endpoints e domínio AP/AR na **7.2**.

## Escopo previsto (7.2+)

- Títulos AP/AR; categorias; centros de custo
- Integração com vendas (`OrderPaidEvent`) e compras XML (`PurchaseInvoiceImportedEvent`) via Core.Application
- Caixa, sangrias, conciliação TEF, DRE e fluxo de caixa (7.3–7.4)

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Agregados financeiros, repositórios, `IFinanceUnitOfWork` |
| [`Application/`](./Application/) | CQRS, handlers MediatR, integração por eventos |
| [`Infrastructure/`](./Infrastructure/) | `FinanceDbContext`, migrations, `AddFinanceModule` |

## Referências

- [`docs/roadmap.md`](../../../docs/roadmap.md) — Fase 7
- [`docs/arquitetura/ADR-001-monolito-modular.md`](../../../docs/arquitetura/ADR-001-monolito-modular.md)
- [`docs/arquitetura/ADR-003-multi-tenancy.md`](../../../docs/arquitetura/ADR-003-multi-tenancy.md)
- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md) — `FinanceIntegrationStatus.Pending` no Sales até 7.2
