using Fiscal.Domain.Enums;
using Fiscal.Domain.Services;

namespace Fiscal.Application.Nfce;

/// <summary>
/// Builds deterministic NFC-e contingency XML and access key for PDV offline (Fake-friendly; Zeus signing in production).
/// </summary>
public static class NfceContingencyBuilder
{
    /// <summary>Builds signed XML placeholder, access key (model 65) and QR URL.</summary>
    public static NfceContingencyBuildResult Build(
        string issuerCnpj,
        string issuerUf,
        int series,
        int number,
        FiscalEmissionType emissionType,
        string recipientName,
        string? recipientCpf,
        IReadOnlyList<NfceContingencyLine> lines)
    {
        var cnpj = new string(issuerCnpj.Where(char.IsDigit).ToArray()).PadLeft(14, '0')[..14];
        var mod = "65";
        var tpEmis = ((int)emissionType).ToString();
        var key = BuildAccessKey(issuerUf, cnpj, mod, series, number);
        var qr = $"https://nfce.fazenda.gov.br/qr?p={key}|2|{tpEmis}|1";
        var xml = $"""
                   <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                     <infNFe Id="NFe{key}" versao="4.00">
                       <ide><mod>{mod}</mod><serie>{series}</serie><nNF>{number}</nNF><tpEmis>{tpEmis}</tpEmis></ide>
                       <emit><CNPJ>{cnpj}</CNPJ></emit>
                       <dest><xNome>{recipientName}</xNome>{(string.IsNullOrWhiteSpace(recipientCpf) ? "" : $"<CPF>{recipientCpf}</CPF>")}</dest>
                     </infNFe>
                   </NFe>
                   """;

        foreach (var line in lines)
        {
            var cfop = FiscalTaxResolver.ResolveProductCfop(issuerUf, recipientCpf is null ? issuerUf : issuerUf);
            xml += $"\n<!-- item {line.Description} cfop {cfop} ncm {line.Ncm} -->";
        }

        return new NfceContingencyBuildResult(key, xml, qr);
    }

    private static string BuildAccessKey(string uf, string cnpj, string mod, int series, int number)
    {
        var ufCode = uf switch
        {
            "SP" => "35",
            "RJ" => "33",
            _ => "35"
        };
        var aamm = DateTime.UtcNow.ToString("yyMM");
        var serie = series.ToString().PadLeft(3, '0')[..3];
        var nnf = number.ToString().PadLeft(9, '0')[..9];
        var tpEmis = "9";
        var body = $"{ufCode}{aamm}{cnpj}{mod}{serie}{nnf}{tpEmis}{number % 10}";
        body = body.PadRight(43, '0')[..43];
        var dv = (body.Sum(c => c - '0') % 10).ToString();
        return body + dv;
    }
}

/// <summary>Product line input for contingency XML.</summary>
public sealed record NfceContingencyLine(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string Ncm);

/// <summary>Output of contingency build.</summary>
public sealed record NfceContingencyBuildResult(string AccessKey, string SignedXml, string QrCodeUrl);
