# `src/Modules/Fiscal/` — Módulo Fiscal

Módulo responsável pela **emissão de documentos fiscais** brasileiros, especialmente NF-e (Nota Fiscal Eletrônica) e NFS-e (Nota Fiscal de Serviços Eletrônica), em conformidade com a legislação tributária brasileira.

## Status

> 🔴 **Não iniciado.** As subpastas de camada existem mas estão vazias (apenas `.gitkeep` e arquivos de projeto).

## Escopo de Negócio

Este módulo gerenciará:
- **NF-e (produto)**: emissão para vendas de mercadorias (rações, acessórios, medicamentos)
- **NFS-e (serviço)**: emissão para serviços prestados (consulta veterinária, banho, tosa)
- **Cancelamento e inutilização**: cancelamento de notas dentro do prazo legal
- **DANFE**: geração do documento auxiliar em PDF
- **XML de notas**: armazenamento dos XMLs assinados para obrigações legais
- **Tributação**: configuração de CFOP, CST, CSOSN, alíquotas de ICMS, PIS, COFINS por produto

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades: `FiscalDocument`, `FiscalDocumentItem`. Value Objects: `TaxCode`, `AccessKey`. Enums: `DocumentType`, `DocumentStatus`. |
| [`Application/`](./Application/) | Commands: `IssueFiscalDocument`, `CancelFiscalDocument`. Queries: `GetDocumentByAccessKey`, `GetPendingDocuments`. |
| [`Infrastructure/`](./Infrastructure/) | `FiscalDbContext`, cliente de integração com SEFAZ (webservice SOAP), geração e assinatura de XML, geração de PDF do DANFE. |

## Dependências

- Integra-se ao `Sales` (recebe dados do pedido para compor a nota)
- Integra-se ao `Inventory` (dados dos produtos: NCM, CFOP, tributação)
- Exige certificado digital A1/A3 para assinatura dos XMLs (configuração da `Infrastructure`)

## Atenção Regulatória

> ⚠️ Este módulo envolve conformidade legal. Toda mudança de regra de tributação deve ser documentada com referência à legislação ou ao comunicado da SEFAZ correspondente.
