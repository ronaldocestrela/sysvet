# ADR 003: Abordagem Multi-Tenancy

## Status
Aceito

## Contexto
O sistema precisa atender a múltiplas clínicas (tenants) de forma segura, garantindo isolamento total dos dados entre os clientes.

## Decisão
Utilizaremos a abordagem de **Schema Separado por Tenant** no SQL Server, orquestrado via EF Core.

## Consequências
- **Positivas**: Isolamento robusto de dados sem o custo de infraestrutura de bancos de dados totalmente separados; Facilita backup e restore granular por cliente se necessário; Menor risco de vazamento acidental de dados em consultas.
- **Negativas**: Migrations do EF Core exigirão um gerenciamento mais complexo para aplicar alterações em todos os schemas; Pode haver um limite prático na quantidade de schemas num único banco antes que a performance administrativa degrade (embora suporte milhares facilmente).
