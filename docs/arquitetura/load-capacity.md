# Carga multi-tenant — Fase 10.7

Meta ADR-060: **50 tenants `Active`** no mesmo SQL Server; aceite de staging **10 tenants × 5 usuários** (50 VUs), 5 min, rotas críticas da 10.4.

SLO: P95 &lt; 500 ms, erro HTTP &lt; 1%, zero vazamento cross-tenant.

## Pré-requisitos

- Staging com SQL Server + Redis (`Cache:Provider=Redis`).
- 10 tenants com dados representativos e 5 tokens JWT staff por tenant.
- Ferramenta [k6](https://k6.io/) instalada.

## Execução

```bash
export BASE_URL="https://staging-api.example"
export TENANTS="token1,token2,...,token50"
export VUS_PER_TENANT=5
export DURATION=5m
k6 run scripts/k6/critical-endpoints.js
```

Registre P50/P95/P99 e taxa de erro abaixo.

### Resultado (colar após execução)

| Cenário | P95 (ms) | Erro % | Data | Observação |
|---------|----------|--------|------|------------|
| 10×5 VUs, rotas 10.4 | | | | |

## CI (guardrail reduzido)

Projeto [`tests/LoadTests`](../../tests/LoadTests/LoadTests.csproj): **3 tenants × 2 requisições concorrentes** in-process.

```bash
dotnet test tests/LoadTests/LoadTests.csproj --filter MultiTenant_ThreeByTwoConcurrent
```

Baseline sequencial 10.4: [`load-baseline.md`](./load-baseline.md).
