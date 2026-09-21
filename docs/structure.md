# Estrutura de Pastas e Diretórios (v1.0)

Este documento detalha a estrutura de pastas e diretórios da primeira versão do SaaS Veterinário e Petshop, seguindo os princípios de Clean Architecture e Monólito Modular baseados no ecossistema .NET 10.

/
├── .github/workflows/ci.yml            # Pipeline GitHub Actions (restore, build, test, cobertura, publish API).
├── global.json                         # Versão do SDK .NET 10 para build reprodutível.
├── coverage.runsettings                  # Filtros de cobertura (Domain + Application) para testes e CI.
├── SaaS_Veterinario.ci.slnf            # Solução filtrada para CI/Linux (exclui MauiApp).
├── scripts/assert-coverage.sh          # Gate de cobertura mínima usado no CI.
├── .dockerignore                       # Contexto enxuto para build de container da API.
├── docs/                               # Arquitetura detalhada, diagramas de domínio e registros de decisão (ADRs).
│   ├── arquitetura/
│   │   ├── README.md                   # Índice de ADRs e template MADR.
│   │   ├── configuracao.md             # Options, health, correlation id (Fase 1.4–1.5).
│   │   ├── sync-poc.md                 # PoC E2E offline → nuvem (Fase 3.6).
│   │   ├── ADR-001-monolito-modular.md
│   │   ├── ADR-002-estrategia-de-sync.md
│   │   ├── ADR-003-multi-tenancy.md
│   │   ├── ADR-004-padrao-cqrs.md
│   │   └── ADR-005-result-http.md
│   ├── diagramas/
│   │   ├── c4-context.mmd              # C4 nível 1 — contexto.
│   │   ├── c4-containers.mmd           # C4 nível 2 — containers vs src/.
│   │   ├── c4-api-components.mmd       # C4 nível 3 — módulos na API.
│   │   └── sync-sequence.mmd           # Outbox: cliente → API → banco nuvem.
│   ├── agents.md
│   ├── structure.md
│   └── roadmap.md
│
├── src/                                # Raiz do código-fonte.
│   ├── API/                            # Projeto ASP.NET Core Web API (Ponto de entrada, injeção de dependência e configuração do Scalar).
│   │   ├── Program.cs                  
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Staging.json
│   │   ├── appsettings.Production.json
│   │   ├── Dockerfile                  # Imagem multi-stage (SDK → aspnet) validada no CI.
│   │   └── Extensions/                 
│   │
│   ├── Clients/                        # Aplicativos clientes.
│   │   ├── BlazorWeb/                  # Aplicação WebAssembly PWA.
│   │   ├── MauiApp/                    # Aplicação Mobile/Desktop MAUI.
│   │   ├── Clients.Infrastructure/     # ApiClient, OfflineDbContext, CRM stores, sync outbox.
│   │   └── SharedUI/                   # RCL — design system (layout, tokens, componentes, serviços UI).
│   │       ├── Components/             # DataGrid, FormField, Modal, Toast, LoadingState, …
│   │       ├── Layout/                 # MainLayout, AuthLayout, NavMenu.
│   │       ├── Navigation/             # AppRoutes, AppNavItems (sem magic strings).
│   │       ├── Services/               # IAuthState, INavigationService, IToastService, …
│   │       ├── DependencyInjection/    # AddSharedUI().
│   │       └── wwwroot/css/app.css     # Tokens VetNexus (SSOT visual).
│   │
│   └── Modules/                        # Contém os módulos de negócio isolados.
│       ├── Core/                       # Módulo base para gestão de acessos e sincronização.
│       │   ├── Domain/                 # Entidades, Value Objects e interfaces de repositório.
│       │   ├── Application/            # Handlers CQRS, DTOs e validações.
│       │   └── Infrastructure/         # EF Core DbContext, Mapeamentos, Repositórios, DependencyInjection.cs (Add*Module), Configuration/ (Options).
│       │
│       ├── Veterinary/                 # Prontuários, internações e vacinas.
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       ├── Petshop/                    # Estética, banho e tosa.
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       ├── Sales/                      # PDV offline e comissões.
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       ├── Inventory/                  
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       ├── Finance/                     
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
│       └── Fiscal/                     
│           ├── Domain/
│           ├── Application/
│           └── Infrastructure/
│
├── tests/                              # Espelho da pasta src/ contendo os testes unitários e de integração.
│   ├── Modules/
│   │   ├── Core.Tests/                 
│   │   ├── Veterinary.Tests/
│   │   └── ... 
│   ├── API.IntegrationTests/           
│   └── Clients.Tests/                  
│
└── SaaS_Veterinario.slnx