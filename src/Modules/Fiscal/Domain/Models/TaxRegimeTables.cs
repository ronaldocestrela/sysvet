namespace Fiscal.Domain.Models;

/// <summary>
/// Simplified LC 123 and Lucro Presumido rates for managerial simulation (7.7).
/// Values are effective percentages on gross revenue for the matched RBT12 bracket.
/// </summary>
public static class TaxRegimeTables
{
    /// <summary>First Simples bracket upper bound (Anexo I and III).</summary>
    public const decimal SimplesFirstBracketMaxRbt12 = 180_000m;

    /// <summary>Second Simples bracket upper bound.</summary>
    public const decimal SimplesSecondBracketMaxRbt12 = 360_000m;

    /// <summary>Anexo I — commerce, first bracket effective rate.</summary>
    public const decimal SimplesAnexoIGoodsFirstBracketRate = 0.04m;

    /// <summary>Anexo I — commerce, second bracket effective rate.</summary>
    public const decimal SimplesAnexoIGoodsSecondBracketRate = 0.073m;

    /// <summary>Anexo I — commerce, third bracket effective rate (simplified).</summary>
    public const decimal SimplesAnexoIGoodsThirdBracketRate = 0.095m;

    /// <summary>Anexo III — services, first bracket effective rate.</summary>
    public const decimal SimplesAnexoIIIServicesFirstBracketRate = 0.06m;

    /// <summary>Anexo III — services, second bracket effective rate.</summary>
    public const decimal SimplesAnexoIIIServicesSecondBracketRate = 0.112m;

    /// <summary>Anexo III — services, third bracket effective rate (simplified).</summary>
    public const decimal SimplesAnexoIIIServicesThirdBracketRate = 0.135m;

    /// <summary>Presumed profit base for goods (IRPJ).</summary>
    public const decimal PresumidoGoodsIrpjBaseRate = 0.08m;

    /// <summary>Presumed profit base for goods (CSLL).</summary>
    public const decimal PresumidoGoodsCsllBaseRate = 0.12m;

    /// <summary>Presumed profit base for services (IRPJ and CSLL).</summary>
    public const decimal PresumidoServicesBaseRate = 0.32m;

    /// <summary>IRPJ rate on presumed profit (cumulativo).</summary>
    public const decimal PresumidoIrpjRate = 0.15m;

    /// <summary>CSLL rate on presumed profit (cumulativo).</summary>
    public const decimal PresumidoCsllRate = 0.09m;

    /// <summary>PIS cumulativo on gross revenue.</summary>
    public const decimal PresumidoPisRate = 0.0065m;

    /// <summary>COFINS cumulativo on gross revenue.</summary>
    public const decimal PresumidoCofinsRate = 0.03m;

    /// <summary>Resolves Anexo I effective rate from rolling goods revenue.</summary>
    public static decimal ResolveSimplesGoodsRate(decimal rbt12Goods)
    {
        if (rbt12Goods <= SimplesFirstBracketMaxRbt12)
        {
            return SimplesAnexoIGoodsFirstBracketRate;
        }

        if (rbt12Goods <= SimplesSecondBracketMaxRbt12)
        {
            return SimplesAnexoIGoodsSecondBracketRate;
        }

        return SimplesAnexoIGoodsThirdBracketRate;
    }

    /// <summary>Resolves Anexo III effective rate from rolling services revenue.</summary>
    public static decimal ResolveSimplesServicesRate(decimal rbt12Services)
    {
        if (rbt12Services <= SimplesFirstBracketMaxRbt12)
        {
            return SimplesAnexoIIIServicesFirstBracketRate;
        }

        if (rbt12Services <= SimplesSecondBracketMaxRbt12)
        {
            return SimplesAnexoIIIServicesSecondBracketRate;
        }

        return SimplesAnexoIIIServicesThirdBracketRate;
    }
}
