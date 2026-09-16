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
│   ├── diagramas/                      
│   ├── agents.md                       
│   ├── structure.md                    
│   └── roadmap.md                      
│
├── src/                                # Raiz do código-fonte.
│   ├── API/                            # Projeto ASP.NET Core Web API (Ponto de entrada, injeção de dependência e configuração do Scalar).
│   │   ├── Program.cs                  
│   │   ├── appsettings.json
│   │   ├── Dockerfile                  # Imagem multi-stage (SDK → aspnet) validada no CI.
│   │   └── Extensions/                 
│   │
│   ├── Clients/                        # Aplicativos clientes.
│   │   ├── BlazorWeb/                  # Aplicação WebAssembly PWA.
│   │   ├── MauiApp/                    # Aplicação Mobile/Desktop MAUI.
│   │   └── SharedUI/                   # Componentes Razor reutilizáveis.
│   │
│   └── Modules/                        # Contém os módulos de negócio isolados.
│       ├── Core/                       # Módulo base para gestão de acessos e sincronização.
│       │   ├── Domain/                 # Entidades, Value Objects e interfaces de repositório.
│       │   ├── Application/            # Handlers CQRS, DTOs e validações.
│       │   └── Infrastructure/         # EF Core DbContext, Mapeamentos, Repositórios e Serviços externos.
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