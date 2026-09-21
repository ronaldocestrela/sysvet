namespace Fiscal.Domain.Services;

/// <summary>
/// Resolves default CFOP/CSOSN for Simples Nacional product lines (7.5 scope).
/// </summary>
public static class FiscalTaxResolver
{
    /// <summary>Default CSOSN for non-taxed Simples lines in 7.5.</summary>
    public const string DefaultCsosn = "102";

    /// <summary>
    /// Returns CFOP 5102 when issuer and recipient share the same UF, otherwise 6102.
    /// </summary>
    public static string ResolveProductCfop(string issuerUf, string? recipientUf)
    {
        if (string.IsNullOrWhiteSpace(recipientUf))
        {
            return "5102";
        }

        return string.Equals(issuerUf.Trim(), recipientUf.Trim(), StringComparison.OrdinalIgnoreCase)
            ? "5102"
            : "6102";
    }
}
