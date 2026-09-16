# `src/API/` — ASP.NET Core Web API

Projeto **ponto de entrada** da aplicação server-side. É o único projeto que tem visibilidade de todos os módulos e é responsável por orquestrar a injeção de dependência, o pipeline HTTP e a documentação da API.

## Responsabilidades

- Inicializar o host da aplicação (`Program.cs`)
- Registrar os serviços de todos os módulos via `AddApplicationModules()` (fachada em `Extensions/`, implementação em cada `Modules/*/Infrastructure/DependencyInjection.cs`)
- Configurar o pipeline HTTP (middlewares, HTTPS, autenticação, etc.)
- Expor a documentação OpenAPI via **Scalar** (interface alternativa ao Swagger UI)
- Mapear os endpoints das controllers/minimal APIs

## Arquivos e Pastas

| Arquivo / Pasta | O que é / Para que serve |
|---|---|
| [`Program.cs`](./Program.cs) | Ponto de entrada: DI modular, Scalar (dev), middlewares, health checks (`/health`, `/health/live`, `/health/ready`) e rotas de negócio. |
| [`Middlewares/CorrelationIdMiddleware.cs`](./Middlewares/CorrelationIdMiddleware.cs) | Propaga `X-Correlation-Id` e escopo de log por requisição. |
| [`appsettings.json`](./appsettings.json) | Defaults não secretos: `Database`, `TenancySettings`, `JwtSettings` (sem `Secret` no base). |
| [`appsettings.Development.json`](./appsettings.Development.json) | SQLite local (`ConnectionStrings:DefaultConnection`), JWT de desenvolvimento e logging. |
| [`appsettings.Staging.json`](./appsettings.Staging.json) / [`appsettings.Production.json`](./appsettings.Production.json) | Provider SQL Server; segredos e connection string **somente** via ambiente ou User Secrets. |
| [`API.http`](./API.http) | Arquivo de requisições HTTP para teste manual dos endpoints via REST Client do VS Code. |
| [`Properties/launchSettings.json`](./Properties/launchSettings.json) | Configurações de inicialização local: perfis de execução, URLs, variáveis de ambiente. |
| [`Extensions/`](./Extensions/README.md) | Composição DI, documentação OpenAPI, health checks e mapeamento HTTP por módulo. |

## Convenções

- **Sem lógica de negócio aqui.** Toda regra de domínio fica nos módulos.
- Novos módulos: `AddXxxModule` na Infrastructure do módulo, entrada em `AddApplicationModules()`, `MapXxxEndpoints` e grupo com `ResultEndpointFilter` em `Program.cs`.
- A documentação Scalar está disponível em `/scalar/v1` quando rodando em `Development`.

## Configuração mínima (Development)

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/API
```

Variáveis obrigatórias por ambiente: [`docs/arquitetura/configuracao.md`](../../docs/arquitetura/configuracao.md). User Secrets (`UserSecretsId` no `.csproj`):

```bash
dotnet user-secrets set "JwtSettings:Secret" "<min-16-chars>" --project src/API
```

## Dependências Externas

| Pacote | Motivo |
|---|---|
| `Scalar.AspNetCore` | Interface de documentação de API que substitui o Swagger UI |
| `Microsoft.AspNetCore.OpenApi` | Geração nativa do documento OpenAPI no .NET 10 |
