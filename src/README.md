# `src/` — Raiz do Código-Fonte

Esta pasta contém **todo o código-fonte de produção** do SysVet SaaS. Nada aqui é código de teste.

## Estrutura Interna

| Subpasta | Propósito |
|---|---|
| [`API/`](./API/README.md) | Ponto de entrada da aplicação: ASP.NET Core Web API |
| [`Clients/`](./Clients/README.md) | Aplicativos clientes (web, mobile/desktop, componentes compartilhados) |
| [`Modules/`](./Modules/README.md) | Módulos de negócio isolados (Core, Veterinary, Petshop, Sales, Inventory, Fiscal) |

## Princípio de Organização

A divisão segue a separação entre **hospedagem/entrega** (`API`, `Clients`) e **domínio/lógica de negócio** (`Modules`). Os módulos não conhecem a API e vice-versa — a comunicação ocorre apenas via injeção de dependência configurada em `API/`.

> **Regra de ouro:** Nunca adicione lógica de negócio diretamente em `API/` ou `Clients/`. Toda regra de domínio vive dentro do módulo correspondente em `Modules/`.
