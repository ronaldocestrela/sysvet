# `src/API/` — ASP.NET Core Web API

Projeto **ponto de entrada** da aplicação server-side. É o único projeto que tem visibilidade de todos os módulos e é responsável por orquestrar a injeção de dependência, o pipeline HTTP e a documentação da API.

## Responsabilidades

- Inicializar o host da aplicação (`Program.cs`)
- Registrar os serviços de todos os módulos via extensões em `Extensions/`
- Configurar o pipeline HTTP (middlewares, HTTPS, autenticação, etc.)
- Expor a documentação OpenAPI via **Scalar** (interface alternativa ao Swagger UI)
- Mapear os endpoints das controllers/minimal APIs

## Arquivos e Pastas

| Arquivo / Pasta | O que é / Para que serve |
|---|---|
| [`Program.cs`](./Program.cs) | Ponto de entrada da aplicação. Cria o `WebApplicationBuilder`, registra serviços e configura o pipeline HTTP. Hoje expõe a documentação Scalar em desenvolvimento e uma rota `GET /` de healthcheck básico. |
| [`appsettings.json`](./appsettings.json) | Configurações de produção (connection strings, log levels, etc.). |
| [`appsettings.Development.json`](./appsettings.Development.json) | Sobrescreve `appsettings.json` em ambiente de desenvolvimento (ex: banco local, log verboso). |
| [`API.http`](./API.http) | Arquivo de requisições HTTP para teste manual dos endpoints via REST Client do VS Code. |
| [`Properties/launchSettings.json`](./Properties/launchSettings.json) | Configurações de inicialização local: perfis de execução, URLs, variáveis de ambiente. |
| [`Extensions/`](./Extensions/README.md) | Classes de extensão de `IServiceCollection` e `IApplicationBuilder` para modularizar o registro de serviços. |

## Convenções

- **Sem lógica de negócio aqui.** Toda regra de domínio fica nos módulos.
- Novos módulos devem ser registrados adicionando um método de extensão em `Extensions/` e chamando-o em `Program.cs`.
- A documentação Scalar está disponível em `/scalar/v1` quando rodando em `Development`.

## Dependências Externas

| Pacote | Motivo |
|---|---|
| `Scalar.AspNetCore` | Interface de documentação de API que substitui o Swagger UI |
| `Microsoft.AspNetCore.OpenApi` | Geração nativa do documento OpenAPI no .NET 10 |
