# `src/Modules/` — Módulos de Negócio

Coração da aplicação. Contém todos os **módulos de domínio** isolados, cada um implementando Clean Architecture internamente. Nenhum módulo conhece ou referencia outro módulo diretamente — a comunicação entre módulos, quando necessária, ocorre via **eventos de domínio** ou **interfaces** definidas no módulo `Core`.

## Módulos Disponíveis

| Módulo | Propósito de Negócio | Status |
|---|---|---|
| [`Core/`](./Core/README.md) | Base transversal: entidades compartilhadas (Tutor, Pet), Value Objects, padrão Result, sincronização e gestão de acesso | 🟡 Em desenvolvimento |
| [`Veterinary/`](./Veterinary/README.md) | Prontuários clínicos, internações, vacinas, prescrições e histórico médico | 🔴 Não iniciado |
| [`Petshop/`](./Petshop/README.md) | Agendamento de banho, tosa e serviços estéticos | 🔴 Não iniciado |
| [`Sales/`](./Sales/README.md) | PDV (Ponto de Venda) com suporte offline, pedidos e comissões | 🔴 Não iniciado |
| [`Inventory/`](./Inventory/README.md) | Controle de estoque de produtos e insumos | 🔴 Não iniciado |
| [`Fiscal/`](./Fiscal/README.md) | Emissão de notas fiscais e integração com sistemas fiscais brasileiros | 🔴 Não iniciado |

## Anatomia de um Módulo

Todo módulo segue **exatamente** esta estrutura de camadas (Clean Architecture):

```
[NomeDoModulo]/
├── Domain/           ← Entidades, Value Objects, interfaces de repositório, regras de negócio puras
├── Application/      ← Handlers CQRS (Commands/Queries), DTOs, validações (FluentValidation)
└── Infrastructure/   ← EF Core DbContext, implementações de repositório, serviços externos
```

### Fluxo de Dependência

```
API  →  Application  →  Domain
              ↑
       Infrastructure
```

- `Domain` não depende de ninguém.
- `Application` depende apenas de `Domain`.
- `Infrastructure` depende de `Domain` e `Application`.
- `API` depende de `Application` e `Infrastructure` (para registro de DI).

## Regras de Isolamento

- Módulos **não referenciam outros módulos** diretamente via `ProjectReference`.
- Entidades compartilhadas entre módulos (ex: `Tutor`, `Pet`) vivem em `Core/Domain` e são consumidas via interface.
- Cada módulo tem seu **próprio DbContext** com seu esquema lógico isolado no banco de dados.
