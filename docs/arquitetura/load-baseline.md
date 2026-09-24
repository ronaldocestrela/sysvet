# Baseline de carga — Fase 10.4

Meta: **P95 &lt; 500 ms** nos endpoints críticos, medido em **staging** com SQL Server + Redis (`Cache:Provider=Redis`).

## Endpoints do cenário

| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/v1/intelligence/dashboard` | Staff + Intelligence |
| GET | `/api/v1/intelligence/reports/abc-products?from=&to=` | Staff + Intelligence |
| GET | `/api/v1/platform/metrics?year=&month=` | Super Admin |
| GET | `/api/v1/inventory/products?page=1&pageSize=20` | Staff |
| GET | `/api/v1/tutors?page=1&pageSize=20` | Staff |
| GET | `/api/v1/financial-titles?page=1&pageSize=20` | Staff + Finance |

## CI (guardrail)

O projeto [`tests/LoadTests`](../../tests/LoadTests/LoadTests.csproj) aquece cada rota e mede 20 requisições **sequenciais** por endpoint (subset acima) contra a API in-process com cache **Memory** e SQLite. Falha se P95 ≥ 500 ms (regressão de latência em estado aquecido).

```bash
dotnet test tests/LoadTests/LoadTests.csproj --filter CriticalEndpoints_P95
```

## Staging manual

1. Configure `Cache:Provider=Redis` e `Cache:ConnectionString` no ambiente.
2. Use um tenant médio com dados representativos (vendas, produtos, títulos).
3. Ferramenta sugerida: `k6`, `hey`, ou Azure Load Testing — 1 tenant, ~20 VUs, 2–5 min.
4. Registre P50/P95/P99 por rota abaixo.

### Resultado (colar após execução)

| Rota | P95 (ms) | Data | Observação |
|------|----------|------|------------|
| `/api/v1/intelligence/dashboard` | | | |
| `/api/v1/tutors` | | | |
| `/api/v1/inventory/products` | | | |
| `/api/v1/financial-titles` | | | |

Carga multi-tenant: Fase **10.7** — [`load-capacity.md`](./load-capacity.md) e ADR-060.
