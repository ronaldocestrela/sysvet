# `docs/` — Documentação Arquitetural

Pasta de **Documentação Viva** do projeto SysVet. Contém a documentação de arquitetura, decisões de design, diagramas e especificações funcionais. Todo documento aqui deve ser mantido sincronizado com o código.

> **Princípio:** A defasagem entre código e documentação é tratada como erro crítico de compilação conceitual. Ao criar, alterar ou remover módulos, entidades ou regras de negócio, o arquivo correspondente **deve** ser atualizado no mesmo commit.

## Arquivos e Pastas

| Arquivo / Pasta | O que é / Para que serve |
|---|---|
| [`agents.md`](./agents.md) | Instruções de sistema para LLMs. Define stack tecnológica, diretrizes de arquitetura, padrões de código e a estrutura de pastas obrigatória do projeto. É o arquivo de contexto primário para qualquer agente de IA trabalhando neste repositório. |
| [`structure.md`](./structure.md) | Mapa visual da estrutura de pastas do projeto com comentários sobre o propósito de cada pasta. Atualizar sempre que a estrutura mudar. |
| [`roadmap.md`](./roadmap.md) | Roadmap detalhado de funcionalidades por módulo, com fases de desenvolvimento, prioridades e épicos planejados. |
| [`backoffice.md`](./backoffice.md) | Especificação das funcionalidades do painel administrativo (backoffice) do SaaS — gestão de clientes, planos, faturamento e suporte. |
| [`functions.md`](./functions.md) | Catálogo de funções e features do sistema por módulo, servindo como referência de escopo para desenvolvimento. |
| [`arquitetura/`](./arquitetura/) | ADRs (MADR), [`README.md`](./arquitetura/README.md) com índice, [`configuracao.md`](./arquitetura/configuracao.md). |
| [`diagramas/`](./diagramas/) | Diagramas-fonte Mermaid (C4 e sequência); renderizar a partir dos `.mmd`, sem PNG sem fonte. |

## Subpastas

### `arquitetura/`
**Architecture Decision Records (ADRs)** — contexto, opções, decisão, consequências e confirmação no código.

| Arquivo | Tema |
|---------|------|
| [`README.md`](./arquitetura/README.md) | Índice e template MADR |
| [`ADR-001-monolito-modular.md`](./arquitetura/ADR-001-monolito-modular.md) | Monólito modular vs microsserviços |
| [`ADR-002-estrategia-de-sync.md`](./arquitetura/ADR-002-estrategia-de-sync.md) | Sync offline (Outbox vs Dotmim.Sync) |
| [`ADR-003-multi-tenancy.md`](./arquitetura/ADR-003-multi-tenancy.md) | Schema por tenant |
| [`ADR-004-padrao-cqrs.md`](./arquitetura/ADR-004-padrao-cqrs.md) | CQRS e MediatR |
| [`ADR-005-result-http.md`](./arquitetura/ADR-005-result-http.md) | `Result<T>` → HTTP |
| [`ADR-014-sqlite-local-clients.md`](./arquitetura/ADR-014-sqlite-local-clients.md) | SQLite CRM offline nos clients |
| [`configuracao.md`](./arquitetura/configuracao.md) | Options, health, correlation id |
| [`sync-poc.md`](./arquitetura/sync-poc.md) | PoC E2E sync offline → nuvem (Fase 3.6) |

### `diagramas/`
Diagramas técnicos em `.mmd` (Mermaid), alinhados a `src/`:

| Arquivo | Nível |
|---------|--------|
| [`c4-context.mmd`](./diagramas/c4-context.mmd) | C4 — contexto do sistema |
| [`c4-containers.mmd`](./diagramas/c4-containers.mmd) | C4 — containers (API, clientes, bancos) |
| [`c4-api-components.mmd`](./diagramas/c4-api-components.mmd) | C4 — componentes dentro da API |
| [`sync-sequence.mmd`](./diagramas/sync-sequence.mmd) | Sequência Outbox offline → nuvem |

## Como Contribuir com a Documentação

1. **Novo módulo ou entidade?** → Atualize `structure.md` e crie um `README.md` na pasta do módulo
2. **Nova decisão arquitetural?** → Crie um ADR em `arquitetura/`
3. **Nova funcionalidade planejada?** → Adicione ao `roadmap.md` com fase e prioridade
4. **Mudança de regra de negócio?** → Atualize `functions.md` e o `README.md` do módulo afetado
