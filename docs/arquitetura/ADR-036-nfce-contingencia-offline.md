# ADR-036: NFC-e modelo 65 e contingência offline no PDV

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 7.6 exige NFC-e consumidor no PDV com contingência offline (`tpEmis=9`), transmissão automática após sync e reconciliação de status SEFAZ. A 7.5 (ADR-035) entregou NF-e 55 e NFS-e Nacional com emissão explícita online. O PDV offline (ADR-026) já persiste vendas e outbox FIFO sem fila fiscal.

## Opções consideradas

1. **Emissão só no servidor após sync** — A1 permanece na nuvem; não atende cupom/QR legal na hora da venda offline.
2. **Contingência legal no PDV (tpEmis=9)** — XML assinado localmente, DANFE NFC-e/QR na venda; transmissão SEFAZ via outbox após `PayOrder` sincronizado.
3. **Reserva de numeração na API** — depende de rede para cada venda; rejeitado (ADR-026).

## Decisão

- **NFC-e modelo 65** via pacote `Zeus.Net.NFe.NFCe`; porta **`INfceGateway`** (separada de `INfeGateway` modelo 55).
- **Contingência offline NFC-e:** `tpEmis=9` no PDV; cliente assina XML e persiste `FiscalDocumentStatus.ContingencyIssued`; **não** chama SEFAZ offline.
- **Gatilho:** automático no pay do PDV quando emitente + certificado A1 estão cacheados localmente; sem certificado a venda segue sem nota.
- **Série NFC-e por dispositivo:** sequência local `(NfceSeries, NextNumber)` no SQLite; default de série a partir do `IssuerProfile`.
- **A1 no PDV:** bundle autenticado (`GET /api/v1/fiscal/issuer/pos-bundle`); PFX e senha cifrados no SQLite do client.
- **Outbox:** após `CreateOrder` e `PayOrder`, enfileirar `TransmitNfceCommand` (FIFO); idempotência via `OutboxMessage.Id`.
- **Pay não bloqueia** falha fiscal; pedido permanece pago (ADR-026).
- **NF-e 55 / NFS-e** permanecem emissão explícita (`IssueFromOrderCommand`); linhas já cobertas por NFC-e autorizada/contingência não geram NF-e 55 duplicada.
- **Tributação inicial:** igual 7.5 (Simples, CSOSN 102, CFOP 5102/6102).
- **Fora desta fatia:** EPEC, FS-DA NF-e 55, inutilização, NFS-e municipal.

## Consequências

- Domain: `FiscalDocumentType.Nfce`, `FiscalEmissionType`, status `ContingencyIssued`, `Order.ConsumerCpf`, `MarkFiscalPending`.
- `FiscalSyncPushHandler` + pull de documentos NFC-e e issuer (sem senha em claro no feed).
- Client: `IFiscalStore` / `OfflineFiscalStore`, hook em `OfflineSalesStore`.
- Testes: Fake `INfceGateway` no CI; homologação SEFAZ com trait `Homologation`.

## Relacionados

- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-026](./ADR-026-pdv-offline-sync.md), [ADR-035](./ADR-035-provedor-fiscal-zeus-openac.md)
- [`docs/diagramas/fiscal-nfce-contingencia.mmd`](../diagramas/fiscal-nfce-contingencia.mmd)
