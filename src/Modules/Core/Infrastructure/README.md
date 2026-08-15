# `src/Modules/Core/Infrastructure/` — Camada de Infraestrutura do Módulo Core

Camada responsável pela **implementação técnica** das abstrações definidas nas camadas de `Domain` e `Application`. Conecta o domínio ao mundo real: banco de dados, ORMs, APIs externas, sistema de arquivos.

## Status Atual

> ⚠️ **Em estruturação.** Contém apenas o `Class1.cs` de placeholder gerado pelo template.

## O que virá aqui

```
Infrastructure/
├── Persistence/
│   ├── CoreDbContext.cs              ← DbContext do EF Core exclusivo para o módulo Core
│   └── Configurations/
│       ├── TutorConfiguration.cs     ← Mapeamento da entidade Tutor para tabela do banco
│       └── PetConfiguration.cs       ← Mapeamento da entidade Pet
├── Repositories/
│   └── TutorRepository.cs           ← Implementação de ITutorRepository usando EF Core
└── DependencyInjection.cs           ← Método de extensão para registrar serviços deste módulo no DI
```

## Regras desta Camada

- ✅ Implementa as interfaces definidas em `Application/Interfaces/` (ex: `ITutorRepository`)
- ✅ O `CoreDbContext` possui apenas as tabelas pertencentes ao módulo Core — nunca tabelas de outros módulos
- ✅ Suporta dois providers: **SQL Server** (nuvem) e **SQLite** (offline MAUI/Blazor WASM), trocado via configuração
- ❌ Nenhuma lógica de negócio aqui — apenas acesso a dados e mapeamento
- ❌ Nunca referenciada diretamente pela camada de `Application`
