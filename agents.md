# Arquivo agents.md - Instruções de Sistema e Arquitetura para LLMs

## Visão Geral do Projeto
[cite_start]Este repositório contém o código-fonte de um SaaS focado na gestão de clínicas veterinárias e petshops, unificando operações clínicas, estéticas, financeiras e fiscais[cite: 59]. [cite_start]O sistema deve operar tanto online quanto offline, com aplicativos para dispositivos móveis, desktops e web[cite: 60]. [cite_start]A arquitetura exige modularidade estrita, permitindo que cada módulo seja comercializado individualmente[cite: 61]. [cite_start]Todo o ecossistema é construído sobre o .NET 10[cite: 62].

## Stack Tecnológica Obrigatória
* [cite_start]**Backend:** ASP.NET Core Web API em .NET 10[cite: 63].
* [cite_start]**Aplicações Desktop/Mobile:** .NET MAUI com compartilhamento de código para Windows, macOS, iOS e Android[cite: 64].
* [cite_start]**Aplicação Web:** Blazor Web App utilizando renderização WebAssembly (PWA)[cite: 65].
* [cite_start]**Banco de Dados (Nuvem):** SQL Server[cite: 66].
* [cite_start]**Banco de Dados (Local/Offline):** SQLite para clientes MAUI e Blazor WebAssembly[cite: 66].
* [cite_start]**ORM:** Entity Framework Core 10[cite: 67].
* [cite_start]**Autenticação/Autorização:** ASP.NET Core Identity Framework[cite: 67].
* [cite_start]**Documentação de API:** OpenAPI configurado exclusivamente com a interface Scalar[cite: 68].

## Diretrizes de Arquitetura (Clean Architecture & Modular Monolith)
* [cite_start]O backend deve iniciar como um Monólito Modular (Modular Monolith)[cite: 69].
* [cite_start]Cada módulo (Core, Estoque, Clínico, Financeiro, etc.) deve ser uma Class Library isolada, contendo seu próprio esquema de banco de dados lógico para garantir independência estrutural[cite: 70].
* [cite_start]Implemente estritamente a Clean Architecture dentro de cada módulo, separando as camadas de Domain, Application, Infrastructure e Presentation[cite: 71].
* [cite_start]Utilize o padrão CQRS (Command Query Responsibility Segregation) para todas as operações de leitura e escrita, facilitando a complexidade da sincronização offline/online[cite: 72].
* [cite_start]Utilize o Repository Pattern para abstrair o acesso a dados, garantindo que a camada de aplicação desconheça o provedor de banco de dados subjacente (SQL Server ou SQLite)[cite: 73].
* [cite_start]Utilize o padrão Result para padronizar retornos de API e operações de aplicação, encapsulando sucesso, falha, mensagem, código de erro e dados sem depender de exceções para fluxo normal de negócio. O contrato deve preferencialmente incluir propriedades como `IsSuccess`, `Error`, `ErrorCode`, `Message` e `Data` (ou `Value`), garantindo consistência entre módulos e simplificando validações, tratamento de erros e testes.[cite: 73].
* [cite_start]Em endpoints, handlers e services, prefira retornar um `Result<T>` em vez de lançar exceções para validações de negócio, regras de autorização, inconsistências de domínio ou falhas de integração; exceções devem ser reservadas para cenários inesperados, de infraestrutura ou de falha crítica do sistema.[cite: 73].

## Diretrizes de Frontend e Reutilização
* [cite_start]Crie uma biblioteca de classes compartilhada (Razor Class Library) contendo componentes de UI agnósticos[cite: 74].
* [cite_start]Maximize a reutilização de componentes Razor entre o Blazor WebAssembly e o .NET MAUI (via Blazor Hybrid)[cite: 75].
* [cite_start]A lógica de estado e regras de interface não devem ser duplicadas entre as plataformas móveis, web e desktop[cite: 76].

## Metodologia e Qualidade de Código (TDD & Comentários)
* [cite_start]**TDD (Test-Driven Development):** Nenhuma funcionalidade ou alteração de código deve ser iniciada sem a escrita prévia do teste unitário correspondente falhando[cite: 77]. [cite_start]A cobertura de testes deve focar intensamente na camada de Domain e Application[cite: 78].
* [cite_start]**Documentação em Código:** Todos os métodos públicos, classes, interfaces e propriedades de domínio devem obrigatoriamente utilizar tags XML[cite: 79]. [cite_start]Explique o "porquê" e não apenas o "o quê"[cite: 80].
* [cite_start]**Clean Code:** Mantenha métodos curtos, nomes descritivos em inglês (para o código estrutural) e evite magic strings[cite: 81].

## Documentação Viva (Living Documentation)
* [cite_start]Este arquivo (`agents.md`) e todos os arquivos de documentação (`.md` na pasta raiz ou nas pastas `/docs`) são considerados Documentação Viva[cite: 82].
* [cite_start]Ao criar, alterar ou remover qualquer módulo, entidade, endpoint ou regra de negócio, o LLM atuante deve atualizar os arquivos Markdown correspondentes imediatamente no mesmo commit ou pull request[cite: 83].
* [cite_start]A defasagem entre o código e a documentação é tratada como erro crítico de compilação conceitual[cite: 84].

## Estrutura de Pastas Obrigatória
* [cite_start]`src/` - Raiz do código-fonte[cite: 86].
* [cite_start]`src/Modules/` - Contém os módulos de negócio isolados[cite: 86].
* [cite_start]`src/Modules/Core/` - Módulo base para gestão de acessos e sincronização[cite: 87].
* [cite_start]`src/Modules/Veterinary/` - Prontuários, internações e vacinas[cite: 88].
* [cite_start]`src/Modules/Petshop/` - Estética, banho e tosa[cite: 88].
* [cite_start]`src/Modules/Sales/` - PDV offline e comissões[cite: 89].
* [cite_start]`src/Modules/[NomeDoModulo]/Domain/` - Entidades, Value Objects e interfaces de repositório[cite: 89].
* [cite_start]`src/Modules/[NomeDoModulo]/Application/` - Handlers CQRS, DTOs e validações[cite: 90].
* [cite_start]`src/Modules/[NomeDoModulo]/Infrastructure/` - EF Core DbContext, Mapeamentos, Repositórios e Serviços externos[cite: 90].
* [cite_start]`src/API/` - Projeto ASP.NET Core Web API (Ponto de entrada, injeção de dependência e configuração do Scalar)[cite: 91].
* [cite_start]`src/Clients/` - Aplicativos clientes[cite: 92].
* [cite_start]`src/Clients/BlazorWeb/` - Aplicação WebAssembly PWA[cite: 92].
* [cite_start]`src/Clients/MauiApp/` - Aplicação Mobile/Desktop MAUI[cite: 92].
* [cite_start]`src/Clients/SharedUI/` - Componentes Razor reutilizáveis[cite: 93].
* [cite_start]`tests/` - Espelho da pasta `src/` contendo os testes unitários e de integração[cite: 93].
* [cite_start]`docs/` - Arquitetura detalhada, diagramas de domínio e registros de decisão (ADRs)[cite: 94].