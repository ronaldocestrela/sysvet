# `src/API/Extensions/` — Composição da API

Métodos de extensão que mantêm o [`Program.cs`](../Program.cs) enxuto: documentação OpenAPI, registro agregado de módulos e mapeamento HTTP por bounded context.

## DI (composition root)

| Arquivo | Responsabilidade |
|---|---|
| [`ServiceCollectionExtensions.cs`](./ServiceCollectionExtensions.cs) | `AddApiDocumentation()`, `AddApplicationModules()` — delega para `Add*Module()` em cada `src/Modules/*/Infrastructure/DependencyInjection.cs` |
| [`HealthCheckExtensions.cs`](./HealthCheckExtensions.cs) | `AddApiHealthChecks()`, `MapApiHealthChecks()` — `/health/live`, `/health/ready`, `/health` (JSON agregado) |

Ordem de registro: **Core** (Identity, JWT, behaviors MediatR) → Veterinary → Inventory → Sales → Petshop (stub) → Fiscal (stub).

## Endpoints

| Extensão | Módulo |
|---|---|
| [`EndpointExtensions.MapCoreEndpoints`](./EndpointExtensions.cs) | Tutors, Pets, Sync |
| [`AuthEndpointsExtensions`](./AuthEndpointsExtensions.cs) | Login / refresh |
| [`VeterinaryEndpointExtensions`](./VeterinaryEndpointExtensions.cs) | Agenda, prontuário, vacinas, orçamentos, `/api/v1/ward-units`, `/api/v1/hospitalizations` (execution-map, medication-orders, administrations) |
| [`InventoryEndpointExtensions`](./InventoryEndpointExtensions.cs) | Produtos e movimentações |
| [`SalesEndpointExtensions`](./SalesEndpointExtensions.cs) | PDV e caixa |
| [`PetshopEndpointExtensions`](./PetshopEndpointExtensions.cs) | Stub |
| [`FiscalEndpointExtensions`](./FiscalEndpointExtensions.cs) | Stub |

Rotas de negócio são mapeadas em um `MapGroup` com [`ResultEndpointFilter`](../Middlewares/ResultEndpointFilter.cs). Falhas usam [`ResultExtensions.ToHttpResult`](./ResultExtensions.cs) / `ToProblemDetails`.

## Convenção

- DI de módulo **não** fica nesta pasta — apenas a fachada `AddApplicationModules`.
- Novo módulo: implementar `DependencyInjection.AddXxxModule` na Infrastructure, referenciar na API e adicionar `MapXxxEndpoints` + chamada em `Program.cs`.
