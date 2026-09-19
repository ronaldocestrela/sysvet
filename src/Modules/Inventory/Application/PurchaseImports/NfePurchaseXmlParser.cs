using Core.Domain;
using Inventory.Domain.ValueObjects;
using PurchaseImportErrors = Inventory.Domain.ErrorCodes.PurchaseImport;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Inventory.Application.PurchaseImports;

/// <summary>
/// Parses inbound NF-e XML (nfeProc or standalone NFe) into a neutral purchase document model.
/// </summary>
public static class NfePurchaseXmlParser
{
    private static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    /// <summary>
    /// Parsed duplicate (installment) for future AP integration.
    /// </summary>
    public sealed record ParsedDuplicate(string Number, DateOnly? DueDate, decimal Amount);

    /// <summary>
    /// One stock line extracted from det/prod (split by rastro when present).
    /// </summary>
    public sealed record ParsedLine(
        int ItemNumber,
        string SupplierProductCode,
        string? Barcode,
        string Description,
        string Ncm,
        string UnitOfMeasure,
        decimal Quantity,
        decimal UnitCost,
        decimal LineTotal,
        string? LotNumber,
        DateTimeOffset? ExpirationDate);

    /// <summary>
    /// Full parse result before persistence.
    /// </summary>
    public sealed record ParsedPurchaseNfe(
        AccessKey AccessKey,
        string EmitterLegalName,
        string EmitterTradeName,
        string EmitterDocument,
        string InvoiceNumber,
        string InvoiceSeries,
        DateTimeOffset IssuedAt,
        decimal TotalAmount,
        IReadOnlyList<ParsedLine> Lines,
        IReadOnlyList<ParsedDuplicate> Duplicates);

    /// <summary>
    /// Parses XML stream into a purchase NF-e model.
    /// </summary>
    public static Result<ParsedPurchaseNfe> Parse(Stream xmlStream)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var reader = XmlReader.Create(xmlStream, settings);
            var doc = XDocument.Load(reader);
            var root = doc.Root;
            if (root is null)
            {
                return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.InvalidXml);
            }

            var nfe = root.Name.LocalName == "NFe"
                ? root
                : root.Element(Ns + "NFe") ?? root.Element("NFe");

            if (nfe is null)
            {
                return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.InvalidXml);
            }

            var infNFe = nfe.Element(Ns + "infNFe") ?? nfe.Element("infNFe");
            if (infNFe is null)
            {
                return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.InvalidXml);
            }

            var accessKeyResult = ResolveAccessKey(root, infNFe);
            if (accessKeyResult.IsFailure)
            {
                return Result.Failure<ParsedPurchaseNfe>(accessKeyResult.Error);
            }

            var ide = infNFe.Element(Ns + "ide") ?? infNFe.Element("ide");
            var emit = infNFe.Element(Ns + "emit") ?? infNFe.Element("emit");
            if (ide is null || emit is null)
            {
                return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.InvalidXml);
            }

            var issuedAt = ParseDateTime(ide.Element(Ns + "dhEmi")?.Value ?? ide.Element("dhEmi")?.Value)
                           ?? DateTimeOffset.UtcNow;

            var totalEl = infNFe.Element(Ns + "total")?.Element(Ns + "ICMSTot")
                          ?? infNFe.Element("total")?.Element("ICMSTot");
            var totalAmount = ParseDecimal(totalEl?.Element(Ns + "vNF")?.Value ?? totalEl?.Element("vNF")?.Value) ?? 0m;

            var lines = ParseLines(infNFe);
            if (lines.Count == 0)
            {
                return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.NoLines);
            }

            var duplicates = ParseDuplicates(infNFe);

            return Result.Success(new ParsedPurchaseNfe(
                accessKeyResult.Value,
                emit.Element(Ns + "xNome")?.Value ?? emit.Element("xNome")?.Value ?? string.Empty,
                emit.Element(Ns + "xFant")?.Value ?? emit.Element("xFant")?.Value ?? string.Empty,
                emit.Element(Ns + "CNPJ")?.Value ?? emit.Element("CNPJ")?.Value ?? string.Empty,
                ide.Element(Ns + "nNF")?.Value ?? ide.Element("nNF")?.Value ?? string.Empty,
                ide.Element(Ns + "serie")?.Value ?? ide.Element("serie")?.Value ?? string.Empty,
                issuedAt,
                totalAmount,
                lines,
                duplicates));
        }
        catch (XmlException)
        {
            return Result.Failure<ParsedPurchaseNfe>(PurchaseImportErrors.InvalidXml);
        }
    }

    private static Result<AccessKey> ResolveAccessKey(XElement root, XElement infNFe)
    {
        var prot = root.Element(Ns + "protNFe") ?? root.Element("protNFe");
        var ch = prot?.Element(Ns + "infProt")?.Element(Ns + "chNFe")?.Value
                 ?? prot?.Element("infProt")?.Element("chNFe")?.Value;
        if (string.IsNullOrWhiteSpace(ch))
        {
            var idAttr = infNFe.Attribute("Id")?.Value;
            if (!string.IsNullOrWhiteSpace(idAttr) && idAttr.StartsWith("NFe", StringComparison.OrdinalIgnoreCase))
            {
                ch = idAttr[3..];
            }
        }

        return AccessKey.Create(ch);
    }

    private static List<ParsedLine> ParseLines(XElement infNFe)
    {
        var result = new List<ParsedLine>();
        var dets = infNFe.Elements(Ns + "det").Concat(infNFe.Elements("det"));
        foreach (var det in dets)
        {
            var nItem = int.TryParse(det.Attribute("nItem")?.Value, out var itemNum) ? itemNum : result.Count + 1;
            var prod = det.Element(Ns + "prod") ?? det.Element("prod");
            if (prod is null)
            {
                continue;
            }

            var cProd = prod.Element(Ns + "cProd")?.Value ?? prod.Element("cProd")?.Value ?? string.Empty;
            var cEan = NormalizeBarcode(prod.Element(Ns + "cEAN")?.Value ?? prod.Element("cEAN")?.Value);
            var xProd = prod.Element(Ns + "xProd")?.Value ?? prod.Element("xProd")?.Value ?? string.Empty;
            var ncm = prod.Element(Ns + "NCM")?.Value ?? prod.Element("NCM")?.Value ?? string.Empty;
            var uCom = prod.Element(Ns + "uCom")?.Value ?? prod.Element("uCom")?.Value ?? "UN";
            var qCom = ParseDecimal(prod.Element(Ns + "qCom")?.Value ?? prod.Element("qCom")?.Value) ?? 0m;
            var vUnCom = ParseDecimal(prod.Element(Ns + "vUnCom")?.Value ?? prod.Element("vUnCom")?.Value) ?? 0m;
            var vProd = ParseDecimal(prod.Element(Ns + "vProd")?.Value ?? prod.Element("vProd")?.Value) ?? qCom * vUnCom;

            var rastros = prod.Elements(Ns + "rastro").Concat(prod.Elements("rastro")).ToList();
            if (rastros.Count == 0)
            {
                result.Add(new ParsedLine(nItem, cProd, cEan, xProd, ncm, uCom, qCom, vUnCom, vProd, null, null));
                continue;
            }

            foreach (var rastro in rastros)
            {
                var nLote = rastro.Element(Ns + "nLote")?.Value ?? rastro.Element("nLote")?.Value;
                var dVal = ParseDateOnly(rastro.Element(Ns + "dVal")?.Value ?? rastro.Element("dVal")?.Value);
                var qLote = ParseDecimal(rastro.Element(Ns + "qLote")?.Value ?? rastro.Element("qLote")?.Value) ?? qCom;
                result.Add(new ParsedLine(
                    nItem,
                    cProd,
                    cEan,
                    xProd,
                    ncm,
                    uCom,
                    qLote,
                    vUnCom,
                    qLote * vUnCom,
                    nLote,
                    dVal));
            }
        }

        return result;
    }

    private static List<ParsedDuplicate> ParseDuplicates(XElement infNFe)
    {
        var cobr = infNFe.Element(Ns + "cobr") ?? infNFe.Element("cobr");
        if (cobr is null)
        {
            return [];
        }

        return cobr.Elements(Ns + "dup").Concat(cobr.Elements("dup"))
            .Select(dup => new ParsedDuplicate(
                dup.Element(Ns + "nDup")?.Value ?? dup.Element("nDup")?.Value ?? string.Empty,
                ParseDateOnlyAsDate(dup.Element(Ns + "dVenc")?.Value ?? dup.Element("dVenc")?.Value),
                ParseDecimal(dup.Element(Ns + "vDup")?.Value ?? dup.Element("vDup")?.Value) ?? 0m))
            .ToList();
    }

    /// <summary>Normalizes NF-e cEAN; treats SEM GTIN as absent.</summary>
    public static string? NormalizeBarcode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.Equals("SEM GTIN", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return trimmed;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    private static DateTimeOffset? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt
            : null;
    }

    private static DateTimeOffset? ParseDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            return new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }

        return null;
    }

    private static DateOnly? ParseDateOnlyAsDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}
