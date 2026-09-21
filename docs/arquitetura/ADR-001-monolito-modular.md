# ADR 001: Monólito modular vs microsserviços

## Status
Accepted

## Data
2026-08-15

## Contexto

O SysVet é um SaaS multi-módulo (clínica, petshop, PDV, estoque, fiscal) que deve operar online e offline, com equipe pequena e entregas incrementais. Precisamos de modularidade comercial (módulos vendáveis separadamente) sem multiplicar deploy, observabilidade e rede distribuída desde o dia zero.

**Drivers:** simplicidade operacional no MVP; Clean Architecture por bounded context; caminho opcional para extrair serviços depois; alinhamento com [`docs/agents.md`](../agents.md) (.NET 10, monólito modular).

## Opções consideradas

1. **Microsserviços desde o início** — deploy e escala independentes por domínio.
   - Prós: isolamento de falha e escala fina.
   - Contras: rede, contratos, sagas, CI/CD e debug muito mais caros; offline-first e sync ficam mais difíceis entre serviços.

2. **Monólito clássico (sem fronteiras de módulo)** — um único projeto ou camadas globais.
   - Prós: velocidade inicial aparente.
   - Contras: acoplamento; impossível vender módulos isolados; extração futura cara.

3. **Monólito modular** — um processo ASP.NET Core, várias class libraries com Domain/Application/Infrastructure, DbContext e DI por módulo.
   - Prós: um artefato para deploy/teste; fronteiras explícitas; preparação para extração.
   - Contras: exige disciplina (sem `ProjectReference` entre módulos de negócio; integração via eventos/contratos no Core).

## Decisão

Adotar **monólito modular** em .NET 10: um host API compõe os módulos `Core`, `Veterinary`, `Petshop`, `Sales`, `Inventory` e `Fiscal` via extension methods de DI e mapeamento de endpoints. Cada módulo mantém esquema lógico próprio no banco. Módulos futuros (`Finance`, `Automations`, `Intelligence`, `TutorPortal`, `Platform`) seguirão o mesmo padrão quando criados.

## Consequências

- **Positivas:** CI com um build de API; testes de integração com `WebApplicationFactory`; evolução por módulo sem rede interna.
- **Negativas:** risco de “monólito distribuído disfarçado” se módulos referenciarem uns aos outros diretamente — mitigado por revisão e (futuro) testes de arquitetura.
- **Futuro:** extração de um módulo para serviço próprio exige API estável e fila/event bus; não é objetivo da Fase 1.

## Confirmação no código

- Composição na API: [`AddApplicationModules()`](../../src/API/Extensions/ServiceCollectionExtensions.cs) registra `AddCoreModule`, `AddVeterinaryModule`, `AddInventoryModule`, `AddSalesModule`, `AddPetshopModule`, `AddFinanceModule`, `AddFiscalModule`.
- Host: [`Program.cs`](../../src/API/Program.cs) — `MapCoreEndpoints`, `MapAuthEndpoints`, `MapVeterinaryEndpoints`, `MapInventoryEndpoints`, `MapSalesEndpoints`, `MapPetshopEndpoints`, `MapFinanceEndpoints`, `MapFiscalEndpoints`; filtro global [`ResultEndpointFilter`](../../src/API/Middlewares/ResultEndpointFilter.cs).
- Estrutura: `src/Modules/{Modulo}/{Domain,Application,Infrastructure}/`.
- Persistência isolada por módulo: `CoreDbContext`, `VeterinaryDbContext`, `InventoryDbContext`, `SalesDbContext`, `PetshopDbContext`, `FinanceDbContext` (Fiscal: DI stub, sem DbContext funcional ainda).
- Integração entre módulos (exemplos): `ConsumeStockForSaleRequest` (Core) tratado em Inventory durante `PayOrderCommand` (Sales), antes do commit; `OrderPaidEvent` enriquecido para Finance futuro e `ClinicalQuoteConvertedEvent` para Veterinary — tudo via MediatR, sem HTTP interno (ADR-025).

## Relacionados

- [ADR-004](./ADR-004-padrao-cqrs.md), [ADR-003](./ADR-003-multi-tenancy.md)
- [c4-containers.mmd](../diagramas/c4-containers.mmd), [c4-api-components.mmd](../diagramas/c4-api-components.mmd)
- [structure.md](../structure.md), [agents.md](../agents.md)
