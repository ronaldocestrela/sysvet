# Status do Projeto, Lacunas e Próximos Passos (SysVet)

> **Data de Atualização:** 15/08/2026  
> **Documento de Referência:** [`agents.md`](file:///home/kley/sysvet/agents.md) e [`roadmap.md`](file:///home/kley/sysvet/roadmap.md)

---

## 1. Diagnóstico do Estado Atual

O projeto SysVet está com sua fundação de solução (`SaaS_Veterinario.slnx`), estrutura de pastas e API base configuradas. O desenvolvimento foi iniciado seguindo a metodologia **TDD (Test-Driven Development)** para a camada de domínio do módulo `Core`.

### 🟢 O que já está implementado

| Componente | Detalhes da Implementação | Arquivos de Referência |
|---|---|---|
| **Arquitetura Base** | .NET 10, Monólito Modular e Clean Architecture | [`SaaS_Veterinario.slnx`](file:///home/kley/sysvet/SaaS_Veterinario.slnx) |
| **API Backend** | ASP.NET Core Web API com OpenAPI + UI Scalar | [`Program.cs`](file:///home/kley/sysvet/src/API/Program.cs), [`API.csproj`](file:///home/kley/sysvet/src/API/API.csproj) |
| **Result Pattern** | Padrão `Result<T>` e `Error` para substituição de exceções em regras de negócio | [`Result.cs`](file:///home/kley/sysvet/src/Modules/Core/Domain/Result.cs) |
| **Domain Core (Entities & VOs)** | Entidades `Entity`, `AggregateRoot`, `Tutor`, `Pet` e VOs `Email`, `Cpf`, `Phone` | [`Core/Domain/`](file:///home/kley/sysvet/src/Modules/Core/Domain) |
| **Testes Unitários (TDD)** | 32 testes unitários cobrindo todos os VOs e Entidades criados | [`Core.Tests.csproj`](file:///home/kley/sysvet/tests/Modules/Core.Tests/Core.Tests.csproj) |

---

## 2. Mapeamento de Lacunas (O que falta construir)

Com base nos requisitos obrigatórios do [`agents.md`](file:///home/kley/sysvet/agents.md), foram identificadas as seguintes lacunas divididas por área:

### 2.1 Módulo Core & Infraestrutura
- 🔴 **Interfaces de Repositório (`Core.Domain`):** `ITutorRepository`, `IPetRepository`, `IUserRepository`.
- 🔴 **Camada de Aplicação e CQRS (`Core.Application`):**
  - DTOs (`TutorDto`, `PetDto`).
  - Commands/Queries e seus Handlers (`RegisterTutorCommand`, `GetTutorByIdQuery`, etc.).
  - Validações de Commands com `FluentValidation`.
- 🔴 **Autenticação & Autorização:** ASP.NET Core Identity Framework e geração/validação de Tokens JWT.
- 🔴 **Persistência (`Core.Infrastructure`):**
  - Mapeamento EF Core (`CoreDbContext`, Mappings).
  - Implementação dos Repositórios reais para SQL Server e SQLite.
- 🔴 **Motor de Sincronização Offline-First (PoC 21 SP):**
  - Fila de sincronização e estrutura de *Outbox Pattern* ou `Dotmim.Sync`.
  - `BackgroundService` para envio de dados pendentes da SQLite local para a Nuvem (SQL Server).

### 2.2 Clientes (Frontend / Mobile)
- 🔴 **.NET MAUI (`src/Clients/MauiApp`):** Inicializar o projeto .NET MAUI e vinculá-lo à solução.
- 🔴 **Blazor WebAssembly (`src/Clients/BlazorWeb`):** Configurar o projeto PWA WebAssembly.
- 🔴 **UI Compartilhada (`src/Clients/SharedUI`):** Criar os componentes Razor reutilizáveis (Layout base, formulários de cadastro de Tutor/Pet, tabelas).

### 2.3 Módulos de Negócio Secundários
- 🔴 **Veterinary:** Prontuário eletrônico, histórico, internação e agenda.
- 🔴 **Inventory:** Cadastro de produtos, movimentação e leitura de código de barras.
- 🔴 **Sales:** PDV offline, fechamento de caixa e cálculo de comissões.
- 🔴 **Petshop:** Ficha de banho e tosa, controle de pacotes e baixa de insumos.
- 🔴 **Fiscal:** Emissão de NF-e, NFS-e e NFC-e com contingência offline.

---

## 3. Próximo Passo Sugerido

### 🎯 Passo Imediato: Implementar CQRS e Repositórios no `Core` via TDD

Seguindo o ciclo **Red → Green → Refactor**, a próxima sequência de tarefas é:

1. **Definir Interfaces de Repositório no `Core.Domain`**
   - Criar `ITutorRepository.cs` e `IPetRepository.cs`.

2. **Criar os Testes dos Handlers em `Core.Tests` (Fase RED)**
   - `RegisterTutorCommandHandlerTests.cs` (usando `NSubstitute` para mockar o repositório).
   - `GetTutorByIdQueryHandlerTests.cs`.

3. **Implementar os Handlers e DTOs em `Core.Application` (Fase GREEN)**
   - `RegisterTutorCommand`, `RegisterTutorCommandHandler`.
   - `GetTutorByIdQuery`, `GetTutorByIdQueryHandler`.

4. **Implementar a Persistência EF Core In-Memory / SQLite (`Core.Infrastructure`)**
   - `CoreDbContext` com configurações de mapeamento das tabelas.
   - Implementação concreta dos repositórios.

5. **Expor os Endpoints na API (`src/API`)**
   - Endpoints Minimal API `/api/tutors` vinculados aos Handlers CQRS.

---

## 4. Justificativa ("Por que este próximo passo?")

1. **Continuidade da Clean Architecture:**  
   O domínio do `Core` (`Tutor` e `Pet`) já possui suas regras de negócio e testes validados. O passo natural seguinte é conectar esse domínio à camada de aplicação via **CQRS**, permitindo orquestrar os casos de uso.

2. **Preparação para a PoC de Sincronização Offline (Fase 1 do Roadmap):**  
   O motor de sincronização offline depende de termos casos de uso de escrita (Commands) e leitura (Queries) bem definidos, salvos em repositórios abstratos que possam rodar tanto em SQLite quanto em SQL Server.

3. **Aderência Estrita ao `agents.md`:**  
   O `agents.md` exige CQRS para todas as operações, Repository Pattern para isolar a infraestrutura de dados e TDD com cobertura intensiva em `Domain` e `Application`.

4. **Desbloqueio dos Clientes (Blazor / MAUI):**  
   Os aplicativos clientes precisam de endpoints de API funcionais ou Handlers de aplicação reutilizáveis para exibir e cadastrar dados de tutores e pets.
