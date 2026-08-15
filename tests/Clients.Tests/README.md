# `tests/Clients.Tests/` — Testes dos Componentes de UI

Projeto de **testes de componentes Blazor** para os projetos em `src/Clients/`. Testa a lógica de renderização, eventos e estado dos componentes Razor sem precisar de um navegador real.

## Status

> ⚠️ **Em estruturação.** Contém apenas `UnitTest1.cs` de placeholder.

## O que virá aqui

```
Clients.Tests/
├── SharedUI/
│   ├── Components/
│   │   └── Forms/
│   │       └── TutorFormTests.cs   ← Testa renderização e validação do formulário de tutor
│   └── Pages/
│       └── HomePageTests.cs
└── BlazorWeb/
    └── Pages/
        └── DashboardTests.cs
```

## Abordagem

- Usa **bunit** para renderizar componentes Razor em memória e inspecionar o DOM resultante
- Simula eventos do usuário (cliques, digitação, submissão de formulários)
- Mockea serviços HTTP usando `MockHttpClient` ou equivalente

## Dependências (futuras)

| Pacote | Propósito |
|---|---|
| `bunit` | Renderização e teste de componentes Razor |
| `xUnit` | Framework de testes |
| `Moq` / `NSubstitute` | Mock de serviços injetados nos componentes |
