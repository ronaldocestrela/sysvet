# ADR-015: Blob storage clínico (anexos)

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 4.3 exige upload de fotos, vídeos e PDFs ligados ao atendimento, com download autorizado (`MedicalRecords.Read`). Binários não devem trafegar no sync offline (ADR-002); apenas metadados (`BlobKey`, MIME, tamanho) sincronizam.

## Opções consideradas
1. **Coluna BLOB no PostgreSQL/SQLite do módulo Veterinary** — simples, mas infla backup/sync e acopla EF a bytes.
2. **Porta `IBlobStorage` no Core + implementações Local/Azure** — desacopla Veterinary de provedor; Local para dev/testes; Azure opcional em produção.
3. **Somente Azure desde o início** — exige credenciais em todo ambiente de dev.

## Decisão
Adotar **`IBlobStorage` em Core.Application** com `BlobStorageOptions` (`Provider`: `Local` | `InMemory` | `Azure`). Chave lógica `{tenantSchema}/clinical/{yyyy}/{MM}/{attachmentId}`. Upload via API multipart (online); pull sync inclui metadados sem bytes.

## Consequências
- Veterinary persiste `ClinicalAttachment` com `BlobKey`; compensação `DeleteAsync` se `SaveChanges` falhar após `PutAsync`.
- Clients listam anexos offline; upload/download chamam API quando online.
- Limites de MIME/tamanho validados em `AttachmentFileSpec` (domínio).

## Referências
- [`configuracao.md`](./configuracao.md) — seção `BlobStorage`
- [`sync-poc.md`](./sync-poc.md) — metadados 4.3
- [`docs/diagramas/exames-receitas-anexos.mmd`](../diagramas/exames-receitas-anexos.mmd)
