# SysVet — SaaS para Clínicas Veterinárias e Petshops

[![CI](https://github.com/ronaldocestrela/sysvet/actions/workflows/ci.yml/badge.svg)](https://github.com/ronaldocestrela/sysvet/actions/workflows/ci.yml)

Sistema de gestão unificado para clínicas veterinárias e petshops, cobrindo operações clínicas, estéticas, financeiras e fiscais. Construído em **.NET 10** com Clean Architecture e Monólito Modular.

## CI/CD

O pipeline [`.github/workflows/ci.yml`](.github/workflows/ci.yml) roda em push/PR para `main` e `develop`: job **Linux** — restore, build e testes em **Release** (`net10.0`) via [`SaaS_Veterinario.ci.slnf`](SaaS_Veterinario.ci.slnf) (sem MAUI); cobertura **70%** Domain/Application; artefato API; Docker. Job **Windows** (`maui-publish`) — publish MAUI Android + Windows.

Para bloquear merge quando o CI falhar: GitHub → **Settings → Branches** → regra em `main`/`develop` → exigir o status check **`build-and-test`**.

### Pré-requisitos locais (Linux / WSL)

Para compilar a solução completa (inclui Blazor WASM com SQLite nativo):

- Workload .NET: `dotnet workload install wasm-tools`
- Ubuntu/Debian: `sudo apt-get install -y libatomic1` (Node do Emscripten usado no link `emcc`; sem essa lib o build falha com exit 127)

## Navegação Rápida

| Área | Link |
|---|---|
| 📐 Arquitetura e Stack | [`docs/agents.md`](./docs/agents.md) |
| 📋 ADRs (decisões) | [`docs/arquitetura/`](./docs/arquitetura/README.md) |
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
└── docs/               ← Documentação viva, ADRs (arquitetura/), diagramas C4 (.mmd)
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
