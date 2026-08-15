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
| [`arquitetura/`](./arquitetura/) | Documentos detalhados de arquitetura: ADRs (Architecture Decision Records), diagramas de sequência, descrição das camadas. |
| [`diagramas/`](./diagramas/) | Diagramas visuais do sistema: ERDs (entidade-relacionamento), diagramas de contexto C4, fluxos de dados. |

## Subpastas

### `arquitetura/`
Destinada a **Architecture Decision Records (ADRs)** — documentos curtos que registram decisões arquiteturais importantes com contexto, alternativas consideradas e justificativa da escolha.

Exemplo de arquivo esperado: `ADR-001-modular-monolith-over-microservices.md`

### `diagramas/`
Destinada a **diagramas técnicos** em formatos como `.puml` (PlantUML), `.drawio` ou `.mmd` (Mermaid). Os diagramas devem ser gerados/renderizados a partir dos arquivos-fonte aqui armazenados, nunca de imagens binárias sem fonte editável.

## Como Contribuir com a Documentação

1. **Novo módulo ou entidade?** → Atualize `structure.md` e crie um `README.md` na pasta do módulo
2. **Nova decisão arquitetural?** → Crie um ADR em `arquitetura/`
3. **Nova funcionalidade planejada?** → Adicione ao `roadmap.md` com fase e prioridade
4. **Mudança de regra de negócio?** → Atualize `functions.md` e o `README.md` do módulo afetado
