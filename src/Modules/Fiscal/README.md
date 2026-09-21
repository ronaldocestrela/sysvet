# `src/Modules/Fiscal/` — Módulo Fiscal (Fase 7.5)

Emissão **online** de **NF-e (modelo 55)** via [Zeus.Net.NFe.NFCe](https://www.nuget.org/packages/Zeus.Net.NFe.NFCe/) e **NFS-e Padrão Nacional** via [OpenAC.Net.NFSe.Nacional.Web](https://www.nuget.org/packages/OpenAC.Net.NFSe.Nacional.Web/), a partir de pedido **pago** (emissão explícita).

## Status

> **7.5 implementada** com gateways **Fake** (CI/dev) e registro **ZeusOpenAc** para homologação. DANFE: `FakeDanfeRenderer` (QuestPDF/Zeus DANFE em evolução).

Ver [ADR-035](../../docs/arquitetura/ADR-035-provedor-fiscal-zeus-openac.md).

## Camadas

| Pasta | Conteúdo |
|---|---|
| [`Domain/`](./Domain/) | `IssuerProfile`, `FiscalDocument`, `FiscalDocumentItem`, `FiscalCorrectionLetter`, `FiscalTaxResolver`, VOs (`FiscalCnpj`, `Cfop`, …). |
| [`Application/`](./Application/) | CQRS: `IssueFromOrder`, cancelamento, CC-e, issuer, downloads; portas `INfeGateway`, `INfseGateway`, `IDanfeRenderer`, `ICertificateProtector`. |
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

Rotas em [`FiscalEndpointExtensions`](../../API/Extensions/FiscalEndpointExtensions.cs): `/api/v1/fiscal/issuer`, `/api/v1/fiscal-documents/*`.

Permissões: `Fiscal.Read`, `Fiscal.Write`. Menu: `fiscal`.

## Integração

- **Sales:** `GetPaidOrderFiscalSnapshotRequest`, `MarkOrderFiscalLinkedRequest`, `FiscalIntegrationStatus`.
- **Inventory:** `GetProductsFiscalBasicsRequest` (NCM/origem).
- **Core:** `GetTutorFiscalDestRequest` (`PostalAddress` no tutor).
- **Blob (ADR-015):** XML/DANFE em `{schema}/fiscal/{yyyy}/{MM}/{documentId}.*`.

Pedido misto: produtos/kits → NF-e; serviços/pacotes → NFS-e Nacional.

## Fora desta fatia

NFC-e/contingência (7.6), inutilização, relatórios 7.7, NFS-e municipal `OpenAC.Net.NFSe`, add-on comercial (9.3).
