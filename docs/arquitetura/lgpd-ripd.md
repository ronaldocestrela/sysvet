# RIPD — SysVet / VetNexus (Fase 10.6)

Registro de Impacto à Proteção de Dados Pessoais (resumo operacional). Documentação viva — atualizar quando fluxos ou bases legais mudarem.

## Papéis

| Papel | Quem |
|-------|------|
| **Controlador** | Clínica/petshop tenant (titular do relacionamento com o tutor) |
| **Operador** | VetNexus (SysVet) — hospedagem, processamento, suporte e billing SaaS |

## Categorias de dados

| Categoria | Exemplos | Onde |
|-----------|----------|------|
| Cadastrais | Nome, CPF, e-mail, telefone, endereço | `Core.Tutors` |
| Clínicos | Prontuário, vacinas, internação | `Veterinary`, blobs [ADR-015](./ADR-015-blob-storage-clinico.md) |
| Fiscais | Destinatário em NF, XML | `Fiscal` (cópia anonimizável; XML histórico mantido) |
| Marketing | Opt-in WhatsApp/e-mail | `Automations.TutorMessagingPreference` |
| Portal | Vínculo Identity ↔ tutor | `TutorPortal` |

## Bases legais (titular tutor)

| Tratamento | Base (LGPD art. 7) |
|------------|-------------------|
| CRM e atendimento | Execução de contrato / procedimentos preliminares |
| Prontuário | Tutela da saúde animal + obrigações profissionais |
| NF-e/NFC-e | Obrigação legal / exercício regular de direito |
| Marketing | Consentimento (`MarketingEnabled`) |
| Billing SaaS (clínica) | Contrato com tenant |

## Retenção

| Dado | Regra |
|------|--------|
| Tutor ativo | Enquanto relação comercial |
| Tutor soft-deleted | Até **5 anos** (`PersonalDataRetentionPolicy`) → candidato à anonimização automática (runbook) |
| Eliminação a pedido | `DELETE /api/v1/privacy/tutors/{id}` — anonimização imediata de identificadores |
| Prontuário / NF | Mantidos anonimizados onde obrigação legal exige guarda |

## Direitos do titular (tutor)

| Direito | Implementação |
|---------|----------------|
| Acesso / portabilidade | `GET /api/v1/privacy/tutors/{id}` (`Privacy.Export`) |
| Eliminação | `DELETE /api/v1/privacy/tutors/{id}` (`Privacy.Erase`) |
| Oposição marketing | Preferências Automations / opt-out existente |

Funcionários da clínica e operadores Super Admin: **fora do escopo** desta API (risco residual — processo manual/DPO).

## Segurança

- **Trânsito:** HTTPS, headers de segurança, rate limit em autenticação ([ADR-059](./ADR-059-lgpd-seguranca-pentest.md)).
- **Repouso:** TDE SQL Server + disco; SQLite local no dispositivo do cliente; segredos (JWT, certificado A1, API keys) via env/user-secrets.
- **CPF em claro:** necessário para unicidade e fiscal; mitigado por TDE e controle de acesso RBAC.

## Transferências / subprocessadores

| Destino | Finalidade |
|---------|------------|
| SMTP / Evolution | Notificações |
| Asaas | Billing SaaS |
| SEFAZ / OpenAC / Zeus | Emissão fiscal |
| Provedor cloud (SQL) | Hospedagem |

Contratos e DPA com tenants e subprocessadores são responsabilidade jurídica/comercial da VetNexus.

## Riscos residuais

1. CPF/e-mail em coluna sem cifra de aplicação (TDE apenas).
2. Prontuário preservado após anonimização do tutor (sem re-identificação direta se tutor anonimizado).
3. Pentest externo periódico depende de contratação (runbook).
