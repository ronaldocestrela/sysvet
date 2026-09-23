# ADR-051: NFS-e do SaaS e impersonation auditada (9.6)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 9.4–9.5 entrega cobrança Asaas/Fake, liquidação em `BillingInvoiceSettlement.ApplyPaidAsync` e bloqueio operacional (ADR-049, ADR-050). O backoffice §3 exige NFS-e da VetNexus contra a clínica a cada liquidação de assinatura; §1 exige impersonation de suporte com trilha imutável e sessão temporária. UI Super Admin permanece 9.8; auditoria ampla de login/API keys permanece 9.7.

A NFS-e tenant-side (OpenAC ADN) vive no módulo Fiscal (ADR-035) e **não** deve ser acoplada ao Platform.

## Opções consideradas

1. **Reutilizar `INfseGateway` / FiscalDbContext** — acopla billing SaaS ao contexto fiscal da clínica; rejeitado.
2. **NFS-e SaaS no Platform com `ISaasNfseGateway` dedicado** — emissor VetNexus em config `Platform:Nfse`; tomador = matriz (`Branch.IsHeadquarters`); escolhido.
3. **Impersonation via `AuditLog` do Core** — consultável pelo Admin do tenant; rejeitado para trilha Super Admin.
4. **`ImpersonationSession` + `ImpersonationAuditEntry` append-only em dbo** — escolhido.

## Decisão

1. **`SaasServiceInvoice`** (dbo, único por `BillingInvoiceId`): emissão após `SaveChanges` da liquidação; falha fiscal **não** reverte pagamento; fatura `Amount == 0` não gera nota.
2. **`ISaasNfseGateway`**: `Fake` (default) ou `OpenAc` (OpenAC Nacional Web, certificado/dados VetNexus em `Platform:Nfse`; validação de endereço do tomador no OpenAc).
3. **Blob** XML em `platform/nfse/{invoiceId}.xml` via `IBlobStorage`.
4. **Retry** `POST .../billing/invoices/{invoiceId}/nfse` (SuperAdmin).
5. **Impersonation**: JWT curto (`Platform:Impersonation:SessionMinutes`, padrão 15), claims `TenantId` alvo, role `Admin`, `impersonation_session_id`; sem refresh token.
6. **Middleware** valida sessão ativa/não expirada em requests com claim de impersonation.
7. **Encerramento** `POST /api/v1/platform/impersonation/{sessionId}/end`; audit `Started`/`Ended` somente insert.

## Consequências

- Aceite: NFS-e na liquidação (Fake); impersonation gera audit imutável e sessão expira.
- Estorno NFS-e no `PAYMENT_REFUNDED` fora de escopo.
- Endereço da matriz opcional no Fake; obrigatório para OpenAc autorizar.

## Confirmação no código

- Domínio: `SaasServiceInvoice`, `ImpersonationSession`, `ImpersonationAuditEntry`
- Application: `IssueSaasNfseForInvoiceCommand`, impersonation commands
- Infra: gateways Fake/OpenAc, migration, `ImpersonationSessionMiddleware`
- API: rotas platform impersonation e retry NFS-e

## Relacionados

- [ADR-049](./ADR-049-gateway-assinatura-asaas.md), [ADR-050](./ADR-050-dunning-bloqueio-cupons.md), [ADR-035](./ADR-035-provedor-fiscal-zeus-openac.md)
- [roadmap](../roadmap.md) §9.6, [backoffice](../backoffice.md) §1 e §3
