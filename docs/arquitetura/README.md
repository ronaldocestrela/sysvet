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
| [ADR-006](./ADR-006-domain-events.md) | Eventos de domínio vs integração | Accepted |
| [ADR-007](./ADR-007-jwt-rbac.md) | JWT Bearer, refresh hash e RBAC | Accepted |
| [ADR-008](./ADR-008-soft-delete-crm.md) | Soft delete CRM (Tutor/Pet) | Accepted |
| [ADR-009](./ADR-009-access-profiles.md) | Perfis de acesso e matriz de permissões | Accepted |
| [ADR-010](./ADR-010-auditoria-openapi.md) | Auditoria append-only e OpenAPI v1 | Accepted |
| [ADR-011](./ADR-011-sharedui-design-system.md) | SharedUI RCL — tokens, layout, componentes | Accepted |
| [ADR-012](./ADR-012-blazor-pwa-jwt.md) | Blazor WASM PWA, JWT cliente, CORS | Accepted |
| [ADR-013](./ADR-013-maui-blazor-hybrid.md) | MAUI Blazor Hybrid, JWT compartilhado, CI Windows | Accepted |
| [ADR-014](./ADR-014-sqlite-local-clients.md) | SQLite local CRM nos clients (EF, IndexedDB WASM) | Accepted |

Documentação complementar (não ADR): [configuracao.md](./configuracao.md) — options, health checks, correlation id; [sync-poc.md](./sync-poc.md) — PoC E2E offline → nuvem (Fase 3.6).

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
