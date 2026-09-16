# Architecture Decision Records (ADRs)

Registros curtos de decisões arquiteturais do SysVet / VetNexus. Mantidos como **documentação viva** — atualize no mesmo commit quando o código mudar a decisão ou sua implementação.

## Índice

| ADR | Título | Status |
|-----|--------|--------|
| [ADR-001](./ADR-001-monolito-modular.md) | Monólito modular vs microsserviços | Accepted |
| [ADR-002](./ADR-002-estrategia-de-sync.md) | Estratégia de sync offline (Outbox vs Dotmim.Sync) | Accepted |
| [ADR-003](./ADR-003-multi-tenancy.md) | Multi-tenancy (schema por tenant) | Accepted |
| [ADR-004](./ADR-004-padrao-cqrs.md) | CQRS e MediatR | Accepted |
| [ADR-005](./ADR-005-result-http.md) | Mapeamento `Result<T>` para HTTP | Accepted |

Documentação complementar (não ADR): [configuracao.md](./configuracao.md) — options, health checks, correlation id.

Diagramas: [`docs/diagramas/`](../diagramas/) — C4 e sequência de sync.

## Template MADR

Use este esqueleto ao criar um novo ADR (`ADR-NNN-titulo-curto.md`):

```markdown
# ADR NNN: Título

## Status
Proposed | Accepted | Deprecated | Superseded by ADR-XXX

## Data
YYYY-MM-DD

## Contexto
Problema, restrições e drivers de negócio/técnicos.

## Opções consideradas
1. **Opção A** — prós / contras
2. **Opção B** — prós / contras

## Decisão
O que foi escolhido e por quê (uma ou duas frases objetivas).

## Consequências
- Positivas: …
- Negativas: …
- Futuro / pendências: …

## Confirmação no código
- `caminho/arquivo.cs` — …

## Relacionados
- ADR-XXX, [diagrama](../diagramas/…), [agents.md](../agents.md)
```

## Numeração

- ADR-004 e ADR-005 já designam CQRS e HTTP/Result; persistência por ambiente está em `configuracao.md`, não em ADR separado.
- ADRs futuros (ex.: provedor fiscal) recebem o próximo número livre (006+).
