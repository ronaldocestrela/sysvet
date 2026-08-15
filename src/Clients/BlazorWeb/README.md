# `src/Clients/BlazorWeb/` — Aplicação Web (Blazor WebAssembly PWA)

Aplicação de **interface web** do SysVet, executada inteiramente no navegador via WebAssembly. Funciona como **Progressive Web App (PWA)**, podendo ser instalada no desktop e operar offline com dados locais via SQLite.

## Arquivos e Pastas

| Arquivo / Pasta | O que é / Para que serve |
|---|---|
| [`Program.cs`](./Program.cs) | Ponto de entrada da aplicação WebAssembly. Cria o `WebAssemblyHostBuilder`, registra o `App` como componente raiz e configura o `HttpClient` apontando para a API. |
| [`App.razor`](./App.razor) | Componente raiz da aplicação. Define o `Router` do Blazor, que mapeia URLs para os componentes de página em `Pages/`. |
| [`_Imports.razor`](./_Imports.razor) | Importações globais de namespaces para todos os componentes `.razor` do projeto. Evita `@using` repetitivos em cada arquivo. |
| [`BlazorWeb.csproj`](./BlazorWeb.csproj) | Arquivo de projeto. Define dependências (ex: referência a `SharedUI`), tipo de output e configurações de build. |
| [`Layout/`](./Layout/) | Componentes de layout reutilizáveis que definem a estrutura visual das páginas (barra de navegação, menu lateral, etc.). |
| [`Pages/`](./Pages/) | Componentes de página roteáveis (anotados com `@page`). Cada arquivo corresponde a uma rota da aplicação. |
| [`wwwroot/`](./wwwroot/) | Arquivos estáticos servidos pelo servidor: `index.html` (shell HTML), CSS, ícones PWA e bibliotecas JS. |
| [`Properties/launchSettings.json`](./Properties/launchSettings.json) | Configurações de execução local (porta, URL, variáveis de ambiente). |

## Subpastas Detalhadas

### `Layout/`
| Arquivo | Propósito |
|---|---|
| `MainLayout.razor` | Layout principal aplicado a todas as páginas. Inclui a barra de navegação lateral (`NavMenu`) e a área de conteúdo principal. |
| `MainLayout.razor.css` | CSS com escopo isolado (CSS isolation) para o `MainLayout`. Estilos aqui se aplicam apenas a este componente. |
| `NavMenu.razor` | Componente do menu de navegação lateral com os links das seções do sistema. |
| `NavMenu.razor.css` | CSS isolado do `NavMenu`. |

### `Pages/`
Páginas de exemplo geradas pelo template do Blazor WASM. **Serão substituídas** pelas páginas reais do SysVet conforme os módulos forem desenvolvidos.

| Arquivo | Propósito atual |
|---|---|
| `Home.razor` | Página inicial (`/`). Placeholder da tela de dashboard. |
| `Counter.razor` | Exemplo de interatividade Blazor. Remover antes da produção. |
| `Weather.razor` | Exemplo de fetch de dados JSON. Remover antes da produção. |
| `NotFound.razor` | Página exibida quando nenhuma rota corresponde à URL acessada. |

### `wwwroot/`
| Arquivo / Pasta | Propósito |
|---|---|
| `index.html` | Shell HTML da SPA. Único arquivo HTML; o Blazor injeta a aplicação no elemento `#app`. |
| `css/app.css` | CSS global da aplicação web. |
| `favicon.png` / `icon-192.png` | Ícones do app (PWA manifest). |
| `lib/bootstrap/` | Biblioteca Bootstrap incluída via libman para estilização básica de layout. |
| `sample-data/weather.json` | Dados de exemplo para a página `Weather.razor`. Remover antes da produção. |
