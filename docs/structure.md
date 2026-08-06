# Estrutura de Pastas e Diretórios (v1.0)

[cite_start]Este documento detalha a estrutura de pastas e diretórios da primeira versão do SaaS Veterinário e Petshop, seguindo os princípios de Clean Architecture e Monólito Modular baseados no ecossistema .NET 10[cite: 69, 71].

/
[cite_start]├── docs/                               # Arquitetura detalhada, diagramas de domínio e registros de decisão (ADRs)[cite: 94].
│   ├── arquitetura/                    
│   ├── diagramas/                      
│   ├── agents.md                       
│   ├── structure.md                    
│   └── roadmap.md                      
│
[cite_start]├── src/                                # Raiz do código-fonte[cite: 86].
[cite_start]│   ├── API/                            # Projeto ASP.NET Core Web API (Ponto de entrada, injeção de dependência e configuração do Scalar)[cite: 91].
│   │   ├── Program.cs                  
│   │   ├── appsettings.json
│   │   └── Extensions/                 
│   │
[cite_start]│   ├── Clients/                        # Aplicativos clientes[cite: 92].
[cite_start]│   │   ├── BlazorWeb/                  # Aplicação WebAssembly PWA[cite: 92].
[cite_start]│   │   ├── MauiApp/                    # Aplicação Mobile/Desktop MAUI[cite: 92].
[cite_start]│   │   └── SharedUI/                   # Componentes Razor reutilizáveis[cite: 93].
│   │
[cite_start]│   └── Modules/                        # Contém os módulos de negócio isolados[cite: 86].
[cite_start]│       ├── Core/                       # Módulo base para gestão de acessos e sincronização[cite: 87].
[cite_start]│       │   ├── Domain/                 # Entidades, Value Objects e interfaces de repositório[cite: 89].
[cite_start]│       │   ├── Application/            # Handlers CQRS, DTOs e validações[cite: 90].
[cite_start]│       │   └── Infrastructure/         # EF Core DbContext, Mapeamentos, Repositórios e Serviços externos[cite: 90].
│       │
[cite_start]│       ├── Veterinary/                 # Prontuários, internações e vacinas[cite: 88].
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
[cite_start]│       ├── Petshop/                    # Estética, banho e tosa[cite: 88].
│       │   ├── Domain/
│       │   ├── Application/
│       │   └── Infrastructure/
│       │
[cite_start]│       ├── Sales/                      # PDV offline e comissões[cite: 89].
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
[cite_start]├── tests/                              # Espelho da pasta src/ contendo os testes unitários e de integração[cite: 93].
│   ├── Modules/
│   │   ├── Core.Tests/                 
│   │   ├── Veterinary.Tests/
│   │   └── ... 
│   ├── API.IntegrationTests/           
│   └── Clients.Tests/                  
│
└── SaaS_Veterinario.sln