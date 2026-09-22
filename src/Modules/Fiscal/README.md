# `src/Modules/Fiscal/` — Módulo Fiscal (Fases 7.5–7.7)

Emissão **online** de **NF-e (modelo 55)** e **NFS-e Padrão Nacional** a partir de pedido **pago** (emissão explícita). **NFC-e (modelo 65)** no PDV offline em contingência (`tpEmis=9`) com transmissão via sync. **Planejamento fiscal:** relatórios por período e simulação gerencial Simples vs Presumido (7.7).

## Status

> **7.5 + 7.6 + 7.7 implementadas** com gateways **Fake** (CI/dev) e registro **ZeusOpenAc** para homologação. NFC-e: `INfceGateway`, `TransmitNfceCommand`, sync push/pull.

Ver [ADR-035](../../docs/arquitetura/ADR-035-provedor-fiscal-zeus-openac.md), [ADR-036](../../docs/arquitetura/ADR-036-nfce-contingencia-offline.md), [ADR-037](../../docs/arquitetura/ADR-037-planejamento-fiscal.md).

## Camadas

| Pasta | Conteúdo |
|---|---|
| [`Domain/`](./Domain/) | `IssuerProfile`, `FiscalDocument`, `FiscalDocumentItem`, `FiscalCorrectionLetter`, `FiscalTaxResolver`, VOs (`FiscalCnpj`, `Cfop`, …). |
| [`Application/`](./Application/) | CQRS: emissão, NFC-e, issuer, downloads; **Planning:** `GetFiscalPlanningQuery`, export CSV/PDF; portas fiscais e `IFiscalPlanningPdfRenderer`. |
| [`Infrastructure/`](./Infrastructure/) | `FiscalDbContext`, `ZeusNfeGateway`, `OpenAcNacionalWebNfseGateway`, fakes, `AesCertificateProtector`, OpenAC multi-tenant providers. |

## Configuração

```json
"Fiscal": {
  "Provider": "Fake",
  "CertificateEncryptionKey": "<min 32 chars, env/user-secrets>"
}
```

- **Fake** (padrão): `FakeNfeGateway` / `FakeNfseGateway`.
- **ZeusOpenAc**: Zeus NF-e + `AddOpenNFSeNacionalWebMultiTenant` (NFS-e ADN).

Certificado **A1** (PFX): blob `{schema}/fiscal/certificates/{issuerId}.pfx.enc`; senha cifrada no perfil.

## API

Rotas em [`FiscalEndpointExtensions`](../../API/Extensions/FiscalEndpointExtensions.cs): `/api/v1/fiscal/issuer`, `/api/v1/fiscal/planning`, `/api/v1/fiscal-documents/*`.

Permissões: `Fiscal.Read`, `Fiscal.Write`. Menu: `fiscal`.

## Integração

- **Sales:** `GetPaidOrderFiscalSnapshotRequest`, `MarkOrderFiscalLinkedRequest`, `FiscalIntegrationStatus`.
- **Inventory:** `GetProductsFiscalBasicsRequest` (NCM/origem).
- **Core:** `GetTutorFiscalDestRequest` (`PostalAddress` no tutor).
- **Blob (ADR-015):** XML/DANFE em `{schema}/fiscal/{yyyy}/{MM}/{documentId}.*`.

Pedido misto: produtos/kits → NF-e; serviços/pacotes → NFS-e Nacional.

## Fora desta fatia

Inutilização, PGDAS/DCTF/SPED, parse de XML tributário, planejamento offline (sync sem totais/itens), NFS-e municipal `OpenAC.Net.NFSe`, add-on comercial (9.3), Zeus NFC-e real (após Fake verde).
