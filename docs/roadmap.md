# Roadmap de Desenvolvimento (Lógica e Story Points)

[cite_start]Este roadmap define as etapas de desenvolvimento do SaaS Veterinário e Petshop, estimando o esforço em Story Points (SP) utilizando a sequência de Fibonacci (1, 2, 3, 5, 8, 13, 21)[cite: 132].

## Fase 1: Fundação e Prova de Conceito (PoC) Offline
[cite_start]**Objetivo:** Construir a base arquitetural e resolver o gargalo técnico da sincronização de dados (CQRS + Local SQLite para Nuvem SQL Server)[cite: 133].

* [cite_start]**Setup Inicial e CI/CD (8 SP)** [cite: 134]
    * [cite_start]Criação da solução `.sln`, estrutura de pastas `src/`, `tests/` e configuração do `agents.md`[cite: 134].
    * [cite_start]Configuração do ASP.NET Core Web API com OpenAPI (Scalar)[cite: 135].
* [cite_start]**Módulo Core - Identidade e CRM Básico (13 SP)** [cite: 136]
    * [cite_start]Domain/Application de Usuários, Tutores e Pets[cite: 136].
    * [cite_start]Implementação do Identity Framework e JWT[cite: 137].
* [cite_start]**Fundação Clients - MAUI e BlazorWeb com SharedUI (13 SP)** [cite: 137]
    * [cite_start]Criação do projeto MAUI e Blazor WebAssembly[cite: 137].
    * [cite_start]Extração do Layout base para `SharedUI` (Razor Class Library)[cite: 138].
* [cite_start]**Motor de Sincronização Offline-First (21 SP)** [cite: 139]
    * [cite_start]Configuração do SQLite no MAUI/Blazor[cite: 139].
    * [cite_start]Implementação de rotina de *Event Sourcing* ou `Dotmim.Sync` salvando offline e enfileirando dados no `BackgroundService` para envio à nuvem[cite: 140].
    * [cite_start]**Teste de PoC:** Cadastrar tutor offline, ligar internet, sincronizar sem conflitos[cite: 141].

[cite_start]*Total Fase 1: 55 SP* [cite: 142]

---

## Fase 2: O Coração do Negócio (Prontuário, Estoque e PDV)
[cite_start]**Objetivo:** Entregar o valor primário que sustenta o funcionamento diário de uma clínica e petshop[cite: 142].

* [cite_start]**Módulo Veterinário - Prontuário e Agenda (13 SP)** [cite: 143]
    * [cite_start]Agenda clínica básica unificada[cite: 143].
    * [cite_start]Prontuário completo (anamnese, evolução, anexos)[cite: 144].
    * [cite_start]Carteira digital de vacinação (protocolos básicos)[cite: 144].
* [cite_start]**Módulo Inventory - Estoque (13 SP)** [cite: 145]
    * [cite_start]Cadastro de produtos e alertas de validade/quantidade[cite: 145].
    * [cite_start]Registro de entrada e saída[cite: 146].
    * [cite_start]Integração MAUI: Leitura de código de barras pela câmera[cite: 146].
* [cite_start]**Módulo Sales - PDV Offline (21 SP)** [cite: 147]
    * [cite_start]Motor de vendas: Adição de itens, fechamento de carrinho[cite: 147].
    * [cite_start]Funcionamento 100% offline do caixa[cite: 148].
    * [cite_start]Regras de cálculo de comissões (vendedores, veterinários)[cite: 148].

[cite_start]*Total Fase 2: 47 SP* [cite: 149]

---

## Fase 3: Especializações e Fiscal
[cite_start]**Objetivo:** Expandir os serviços para o setor de estética e garantir conformidade com a legislação fiscal[cite: 149].

* [cite_start]**Módulo Petshop - Estética e Banho (13 SP)** [cite: 150]
    * [cite_start]Ficha digital de banho e tosa[cite: 150].
    * [cite_start]Baixa automática de insumos no estoque (shampoo, etc)[cite: 151].
    * [cite_start]Controle de pacotes pré-pagos de banho[cite: 151].
* [cite_start]**Módulo Veterinário - Internação (13 SP)** [cite: 152]
    * [cite_start]Mapa de pacientes internados e leitos[cite: 152].
    * [cite_start]Prescrições com controle de horários[cite: 153].
* [cite_start]**Módulo Fiscal - Integração de Notas (21 SP)** [cite: 153]
    * [cite_start]Emissão de NF-e e NFS-e[cite: 153].
    * [cite_start]Emissão de NFC-e com contingência offline (armazenada e transmitida ao recuperar rede)[cite: 154].
    * [cite_start]Integração com bibliotecas/APIs fiscais (ex: Zeus.Net / Focus NFe)[cite: 155].

[cite_start]*Total Fase 3: 47 SP* [cite: 156]

---

## Fase 4: Expansão (Automações e Portal do Tutor)
[cite_start]**Objetivo:** Retenção de clientes e automação do marketing, reduzindo trabalho manual da equipe[cite: 156].

* [cite_start]**Módulo Automations - Worker Services (13 SP)** [cite: 157]
    * [cite_start]Fila de mensagens (WhatsApp, SMS, E-mail) para retornos, vacinas e aniversários[cite: 157].
    * [cite_start]Notificações em tempo real (Status do banho do pet)[cite: 158].
* [cite_start]**App do Tutor - Dashboard PWA/MAUI (13 SP)** [cite: 159]
    * [cite_start]Login para o tutor final[cite: 159].
    * [cite_start]Visualização da carteira de vacinação digital[cite: 160].
    * [cite_start]Agendamento direto pelo aplicativo[cite: 160].
* [cite_start]**Módulo Intelligence - Dashboards (8 SP)** [cite: 161]
    * [cite_start]Relatórios de curva ABC de clientes e produtos[cite: 161].
    * [cite_start]Fluxo de caixa visual[cite: 162].

[cite_start]*Total Fase 4: 34 SP* [cite: 163]