# SysVet — SaaS para Clínicas Veterinárias e Petshops

Sistema de gestão unificado para clínicas veterinárias e petshops, cobrindo operações clínicas, estéticas, financeiras e fiscais. Construído em **.NET 10** com Clean Architecture e Monólito Modular.

## Navegação Rápida

| Área | Link |
|---|---|
| 📐 Arquitetura e Stack | [`agents.md`](./agents.md) |
| 🗺️ Estrutura de Pastas | [`docs/structure.md`](./docs/structure.md) |
| 🚀 Roadmap | [`docs/roadmap.md`](./docs/roadmap.md) |
| 📊 Status do Projeto | [`status_e_proximos_passos.md`](./status_e_proximos_passos.md) |
| 📁 Código-Fonte | [`src/`](./src/README.md) |
| 🧪 Testes | [`tests/`](./tests/README.md) |
| 📚 Documentação | [`docs/`](./docs/README.md) |

## Visão Geral da Arquitetura

```
sysvet/
├── src/
│   ├── API/            ← ASP.NET Core Web API (ponto de entrada)
│   ├── Clients/
│   │   ├── BlazorWeb/  ← Blazor WASM PWA
│   │   ├── MauiApp/    ← .NET MAUI (Windows/macOS/iOS/Android)
│   │   └── SharedUI/   ← Componentes Razor reutilizáveis
│   └── Modules/
│       ├── Core/       ← Base: Tutor, Pet, Result<T>, Value Objects
│       ├── Veterinary/ ← Prontuários, vacinas, internações
│       ├── Petshop/    ← Banho, tosa, agendamentos
│       ├── Sales/      ← PDV offline, pedidos, comissões
│       ├── Inventory/  ← Estoque de produtos
│       └── Fiscal/     ← NF-e, NFS-e, SEFAZ
├── tests/              ← Espelho de src/ com testes unitários e de integração
└── docs/               ← Documentação viva, ADRs, diagramas
```

## Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10) |
| Web | Blazor WebAssembly (PWA) |
| Mobile/Desktop | .NET MAUI (Blazor Hybrid) |
| Banco (nuvem) | SQL Server + Entity Framework Core 10 |
| Banco (local) | SQLite (modo offline) |
| Autenticação | ASP.NET Core Identity |
| Documentação API | OpenAPI + Scalar |
| Testes | xUnit + Moq |

## Metodologia

- **TDD**: Testes escritos antes do código de produção
- **Clean Architecture**: Domain → Application → Infrastructure (dependências apontando para dentro)
- **CQRS**: Separação de Commands e Queries em todos os módulos
- **Result Pattern**: Sem exceções para fluxo normal de negócio
- **Documentação Viva**: `README.md` em cada pasta, atualizado junto com o código
