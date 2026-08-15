# `tests/API.IntegrationTests/` — Testes de Integração da API

Projeto de **testes de integração** que testa os endpoints HTTP da API de ponta a ponta, incluindo o pipeline completo do ASP.NET Core (middlewares, roteamento, serialização JSON, validações).

## Status

> ⚠️ **Em estruturação.** Contém apenas `UnitTest1.cs` de placeholder.

## O que virá aqui

```
API.IntegrationTests/
├── Fixtures/
│   └── ApiFactory.cs          ← WebApplicationFactory customizada com banco em memória
├── Endpoints/
│   ├── TutorEndpointsTests.cs ← Testa CRUD de tutores via HTTP
│   └── PetEndpointsTests.cs   ← Testa cadastro de pets via HTTP
└── Helpers/
    └── HttpClientExtensions.cs ← Helpers para requisições (GetAsync<T>, PostAsync<T>, etc.)
```

## Abordagem

- Usa `WebApplicationFactory<Program>` para subir a API em memória durante os testes
- Banco de dados substituído por **SQLite in-memory** ou **EF Core In-Memory Provider** via override de DI
- Cada teste é **isolado**: banco resetado entre suites
- Testa o contrato HTTP: status codes, headers, corpo da resposta e mensagens de erro

## Dependências

| Pacote | Propósito |
|---|---|
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` |
| `xUnit` | Framework de testes |
