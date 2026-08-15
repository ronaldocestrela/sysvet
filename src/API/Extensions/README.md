# `src/API/Extensions/` — Extensões de Injeção de Dependência

Pasta dedicada a **métodos de extensão** que organizam e encapsulam o registro de serviços no container de DI da aplicação. O objetivo é manter o `Program.cs` limpo e com responsabilidade única.

## Arquivos

| Arquivo | O que faz |
|---|---|
| [`ServiceCollectionExtensions.cs`](./ServiceCollectionExtensions.cs) | Classe estática `ServiceCollectionExtensions` com o método `AddApiDocumentation()`. Registra o serviço `AddOpenApi()` nativo do .NET 10, que gera o documento OpenAPI consumido pelo Scalar. |

## Padrão de Crescimento

Conforme novos módulos forem adicionados ao sistema, seus serviços de infraestrutura serão registrados aqui. Exemplo esperado:

```
Extensions/
├── ServiceCollectionExtensions.cs   ← documentação e OpenAPI
├── CoreModuleExtensions.cs          ← serviços do módulo Core
├── VeterinaryModuleExtensions.cs    ← serviços do módulo Veterinary
└── ...
```

> Cada módulo deve expor um método de extensão próprio (ex: `AddVeterinaryModule(this IServiceCollection services)`), que será chamado a partir dessa pasta.
