# ADR 008: Soft delete no CRM (Tutor e Pet)

## Status
Accepted

## Data
2026-09-16

## Contexto

Tutores e pets participam de histórico clínico, vendas e sync offline. Exclusão física quebraria integridade referencial e auditoria. O roadmap 2.4 exige soft delete, listagens que ocultam registros excluídos e CRUD REST completo.

## Opções consideradas

1. **Hard delete com cascade** — remove linhas e pets vinculados.
   - Contras: perda de histórico; incompatível com módulos satélites.

2. **Soft delete (`IsDeleted`, `DeletedAt`) + query filter EF** — marca exclusão lógica; consultas padrão ignoram deletados.
   - Prós: alinhado a CRM; permite restauração futura; REST retorna 404 após delete.

3. **Tabela de arquivo separada** — move dados para outro schema.
   - Contras: complexidade desproporcional ao MVP.

## Decisão

Adotar **soft delete** em `Tutor` e `Pet` via interface `ISoftDeletable`, colunas `IsDeleted` e `DeletedAt`, e **um único** `HasQueryFilter` por entidade combinando tenant (ADR-003) e `!IsDeleted`. FK Tutor→Pet usa `DeleteBehavior.Restrict`; exclusão de tutor dispara soft delete em cascata na camada de aplicação (`DeleteTutorCommand`).

## Consequências

- **Positivas:** listagens e GET por id não expõem registros excluídos; unicidade de CPF/e-mail aplica-se apenas a tutores ativos visíveis pelo filtro.
- **Negativas:** reutilizar CPF de tutor soft-deleted exige política futura (reativação ou purge).
- **Confirmação:** migration `AddCrmSoftDeleteAndIndexes`; ADR relacionado a multi-tenancy inalterado.

## Confirmação no código

- [`ISoftDeletable.cs`](../../src/Modules/Core/Domain/ISoftDeletable.cs)
- [`Tutor.cs`](../../src/Modules/Core/Domain/Entities/Tutor.cs), [`Pet.cs`](../../src/Modules/Core/Domain/Entities/Pet.cs)
- [`CoreDbContext.cs`](../../src/Modules/Core/Infrastructure/Persistence/CoreDbContext.cs) — `ConfigureTenantShadowProperty` com filtro tenant + soft delete
- [`DeleteTutorCommandHandler.cs`](../../src/Modules/Core/Application/Tutors/Commands/DeleteTutorCommandHandler.cs), [`DeletePetCommandHandler.cs`](../../src/Modules/Core/Application/Pets/Commands/DeletePetCommandHandler.cs)
- Endpoints: [`TutorEndpointsExtensions.cs`](../../src/API/Extensions/TutorEndpointsExtensions.cs), [`PetEndpointsExtensions.cs`](../../src/API/Extensions/PetEndpointsExtensions.cs)

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md)
- [roadmap](../roadmap.md) — item 2.4
