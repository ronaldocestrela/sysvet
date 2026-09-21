# ADR-035: Provedor fiscal — NF-e Zeus.NET e NFS-e Nacional OpenAC

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 7.5 exige emissão de NF-e (produto) e NFS-e (serviço) a partir de vendas pagas, cancelamento e carta de correção (NF-e), com aceite de NF-e autorizada na SEFAZ em homologação. O módulo Fiscal era stub; Inventory trata apenas entrada de compra (ADR-021). NFC-e modelo 65 e contingência offline no PDV ficaram na 7.6 ([ADR-036](./ADR-036-nfce-contingencia-offline.md)) via porta `INfceGateway` separada de `INfeGateway`.

## Opções consideradas

1. **Focus NFe (SaaS)** — menos código, dependência externa e custo por nota.
2. **Zeus + SOAP SEFAZ direto para tudo** — Zeus não cobre NFS-e municipal/nacional.
3. **Zeus (NF-e 55) + OpenAC.Net.NFSe.Nacional.Web (ADN)** — bibliotecas open source, certificado A1 no tenant, controle no monólito.

## Decisão

- **NF-e modelo 55:** pacote `Zeus.Net.NFe.NFCe`; DANFE via `Zeus.NFe.Danfe.QuestPdf`.
- **NFS-e Padrão Nacional:** pacote `OpenAC.Net.NFSe.Nacional.Web` com `AddOpenNFSeNacionalWebMultiTenant`; `tenantId` = `ITenantContext.TenantId`. Não usar `OpenAC.Net.NFSe` municipal nesta fatia.
- **Bounded context:** emissão em `src/Modules/Fiscal`; portas `INfeGateway` / `INfseGateway` na Application; adapters na Infrastructure.
- **Certificado A1:** PFX cifrado em blob; senha cifrada no `IssuerProfile`. A3 fora de escopo.
- **Emissão explícita** via `IssueFromOrderCommand` em pedido **pago** (não handler automático em `OrderPaidEvent`).
- Pedido misto: NF-e para Product/Kit; NFS-e para Service/Package.
- **Ambiente:** `Fiscal:Provider` = `Fake` (CI/dev default) ou `ZeusOpenAc` (homologação/produção).
- **Persistência:** XML/PDF em `IBlobStorage` (`{schema}/fiscal/...`); `PersistenciaHabilitada = false` no OpenAC.
- **Tributação inicial:** Simples Nacional (CRT 1), CSOSN 102, CFOP 5102/6102 intra/inter UF.
- CC-e apenas NF-e; cancelamento NF-e e evento nacional para NFS-e.

## Consequências

- `FiscalDbContext`, permissões `Fiscal.Read` / `Fiscal.Write`, endpoints `/api/v1/fiscal*`.
- `PostalAddress` opcional no Tutor (Core) para destinatário NF-e.
- `FiscalIntegrationStatus` no pedido (Sales), espelhando Finance.
- Testes de homologação SEFAZ/ADN com `[Trait("Category","Homologation")]` e env `FISCAL_HOMOLOGATION=1`.

## Confirmação no código

- [`AddFiscalModule`](../../src/Modules/Fiscal/Infrastructure/DependencyInjection.cs)
- [`FiscalEndpointExtensions`](../../src/API/Extensions/FiscalEndpointExtensions.cs)
- [`IssueFromOrderCommandHandler`](../../src/Modules/Fiscal/Application/Documents/Commands/IssueFromOrderCommandHandler.cs)

## Relacionados

- [ADR-019](./ADR-019-produtos-lotes-estoque.md), [ADR-021](./ADR-021-entrada-xml-nfe-compra.md), [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-026](./ADR-026-pdv-offline-sync.md)
- [`docs/diagramas/fiscal-nfe-nfse.mmd`](../diagramas/fiscal-nfe-nfse.mmd)
