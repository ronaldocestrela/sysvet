# Mapeamento de Funcionalidades: Backoffice da Plataforma (Super Admin) - VetNexus

Este documento detalha as funções exclusivas para a administração geral do SaaS VetNexus. Diferente do backoffice da clínica (que foca na operação veterinária), este painel é utilizado pela equipe proprietária do sistema para o gerenciamento do ecossistema, clientes, monetização e infraestrutura.

---

> **Nota (9.8):** UI Blazor **`PlatformWeb`** (`src/Clients/PlatformWeb/`) consome `/api/v1/platform/*` com role `SuperAdmin` — tenants, planos (leitura), billing, flags, cupons, health, API keys, impersonation e auditoria (ADR-053).
> **Nota (10.2):** Métricas SaaS globais (MRR, ARR, churn, LTV, CAC, inadimplência) via `/api/v1/platform/metrics` e página **`/metrics`**; gasto de aquisição (`AcquisitionSpend`) para CAC (ADR-055).

> **Nota (9.2):** Onboarding, status de tenant, filiais multi-CNPJ e API `/api/v1/platform/tenants` (role `SuperAdmin`) estão entregues (ADR-047). UI em **PlatformWeb** (9.8).

## 1. Gestão de Tenants (Clientes / Estabelecimentos)
* **Cadastro e Onboarding:** Criação de novas contas para clínicas e petshops (Tenants), com provisionamento automático de banco de dados/schemas e configurações iniciais.
* **Status do Tenant:** Ativação, suspensão, cancelamento e exclusão de contas.
* **Gestão de Filiais:** Possibilidade de vincular múltiplos CNPJs/filiais sob a mesma conta matriz.
* **Monitoramento de Saúde (Health Check):** Visualização do volume de dados, espaço em disco consumido e volume de requisições de cada tenant.
* **Impersonation (Acesso Suporte):** Capacidade da equipe de suporte logar temporariamente como o cliente (com auditoria estrita) para resolução de chamados.

> **Nota (9.6):** `POST /api/v1/platform/tenants/{id}/impersonation` (JWT curto, trilha append-only), `POST /api/v1/platform/impersonation/{sessionId}/end`, `GET /api/v1/platform/impersonation-audits` (ADR-051). UI em **PlatformWeb** (9.8).

---

> **Nota (9.3):** Catálogo de planos/add-ons, assinatura por tenant, flags, pró-rata (`PendingBilling`) e enforcement API/menus estão entregues via `/api/v1/platform/*` (ADR-048). UI em **PlatformWeb** (9.8).

## 2. Gestão de Planos, Módulos e Feature Flags
Como a arquitetura exige modularidade estrita, permitindo que cada módulo seja comercializado individualmente, o backoffice precisa de controle granular:
* **Criação de Planos Base:** Estruturação de pacotes (ex: Starter, Pro, Hospital 24h) com módulos pré-definidos.
* **Controle de Módulos Avulsos (Add-ons):** Habilitação e precificação de módulos específicos (ex: Estética, Fiscal, Automação, PDV Offline).
* **Gestão de Feature Flags:** Ativação e desativação remota de funcionalidades e módulos diretamente para um ou mais tenants em tempo real.
* **Upgrades e Downgrades:** Processamento de mudanças de plano solicitadas pelo cliente com cálculo de pró-rata automático.
* **Gestão de Testes Grátis (Free Trial):** Configuração de dias de teste e bloqueio ou transição automática ao fim do período.

---

> **Nota (9.4):** Gateway Asaas (Fake em CI), cliente/meio de pagamento, cobrança recorrente, webhook e histórico de faturas via `/api/v1/platform/tenants/{id}/billing/*` e `POST /api/v1/platform/webhooks/asaas` (ADR-049).
> **Nota (9.5):** Régua dunning (`ProcessDunningCommand`, `Platform:Dunning`), bloqueio operacional (`BillingStanding.Locked`, filtro API, `/api/v1/billing/*`, SharedUI `/billing/payment`) e cupons (`/api/v1/platform/coupons`, resgate por tenant) entregues (ADR-050).
> **Nota (9.6):** NFS-e SaaS na liquidação (`IssueSaasNfseForInvoiceCommand`, `Platform:Nfse` Fake/OpenAc, retry Super Admin) entregue (ADR-051). UI em **PlatformWeb** (9.8).

## 3. Formas de Pagamentos, Faturamento e Cobrança
* **Integração com Gateway de Pagamento:** Conexão com provedores (ex: Stripe, Pagar.me, Asaas) para processamento de assinaturas do SaaS.
* **Configuração de Formas de Pagamento:** Habilitação de cobranças via Cartão de Crédito (recorrência), Pix, ou Boleto Bancário.
* **Gestão de Inadimplência (Dunning):** Réguas de cobrança automatizadas (e-mails e SMS de aviso), controle de tentativas de retentativa de cartão de crédito.
* **Bloqueio Automático:** Suspensão temporária do acesso do tenant (mantendo apenas tela de pagamento) após X dias de atraso.
* **Emissão de NFS-e do SaaS:** Geração automática da Nota Fiscal de Serviço eletrônica (da empresa dona do SaaS contra a clínica cliente) a cada liquidação de assinatura.
* **Gestão de Cupons e Descontos:** Criação de cupons promocionais para equipes de vendas (com limite de usos, expiração e porcentagem/valor fixo).

---

## 4. Auditoria, Segurança e Logs
* **Logs de Acesso da Plataforma:** Registro detalhado de logins de todos os tenants (IP, dispositivo, localização).
* **Auditoria de Configurações:** Rastreamento de quem alterou planos, permissões ou aplicou descontos dentro do Backoffice Super Admin.
* **Gestão de API Keys:** Controle de chaves de integração para parceiros e contabilidades terceirizadas.

---

## 5. Dashboards e Métricas SaaS (Business Intelligence)
* **Métricas Financeiras Globais:** Acompanhamento de MRR (Receita Mensal Recorrente), ARR (Receita Anual), e fluxo de caixa do sistema — entregue em **10.2** (`GET /api/v1/platform/metrics`, ADR-055).
* **Métricas de Crescimento:** Acompanhamento de Churn Rate (taxa de cancelamento), LTV (Lifetime Value), e CAC (Custo de Aquisição de Clientes) — CAC via lançamento **`AcquisitionSpend`** (sem CRM externo).
* **Análise de Adoção de Módulos:** Identificação de quais módulos complementares são mais e menos utilizados/assinados pelos clientes — Fase **10.3**.
* **Relatórios de Inadimplência:** Visão geral de contas a receber e clientes bloqueados no mês — incluído no snapshot **10.2**.
