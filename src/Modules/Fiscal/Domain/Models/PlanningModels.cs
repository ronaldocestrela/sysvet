namespace Fiscal.Domain.Models;

/// <summary>Tax regime suggested by managerial simulation (not a legal classification).</summary>
public enum SuggestedTaxRegime
{
    /// <summary>Simples Nacional estimated load is lower.</summary>
    Simples,

    /// <summary>Lucro Presumido estimated load is lower.</summary>
    Presumido,

    /// <summary>Estimated loads are equal within rounding tolerance.</summary>
    Tie
}

/// <summary>CFOP aggregation line for goods revenue in a period.</summary>
public sealed record CfopBreakdownLine(string Cfop, decimal GrossAmount, int DocumentCount);

/// <summary>Period tax apuration from authorized and cancelled fiscal documents.</summary>
public sealed class FiscalPeriodReport
{
    /// <summary>Report start (inclusive).</summary>
    public DateOnly From { get; init; }

    /// <summary>Report end (inclusive).</summary>
    public DateOnly To { get; init; }

    /// <summary>Net goods revenue (NF-e + NFC-e) after in-period cancellations.</summary>
    public decimal GoodsRevenue { get; init; }

    /// <summary>Net services revenue (NFS-e) after in-period cancellations.</summary>
    public decimal ServicesRevenue { get; init; }

    /// <summary>Goods plus services net revenue.</summary>
    public decimal TotalGrossRevenue => GoodsRevenue + ServicesRevenue;

    /// <summary>Estimated ISS on services using issuer default rate.</summary>
    public decimal EstimatedIss { get; init; }

    /// <summary>Authorized documents counted in the period.</summary>
    public int AuthorizedDocumentCount { get; init; }

    /// <summary>Cancellations recorded in the period (estorno).</summary>
    public int CancelledDocumentCount { get; init; }

    /// <summary>Goods revenue grouped by CFOP from product lines.</summary>
    public IReadOnlyList<CfopBreakdownLine> CfopBreakdown { get; init; } = Array.Empty<CfopBreakdownLine>();
}

/// <summary>Managerial Simples vs Presumido comparison for the period revenue.</summary>
public sealed class RegimeSimulationResult
{
    /// <summary>Estimated Simples Nacional DAS for the period.</summary>
    public decimal SimplesEstimatedTax { get; init; }

    /// <summary>Estimated Lucro Presumido federal + ISS for the period.</summary>
    public decimal PresumidoEstimatedTax { get; init; }

    /// <summary>Effective Simples rate on total period revenue.</summary>
    public decimal SimplesEffectiveRate { get; init; }

    /// <summary>Effective Presumido rate on total period revenue.</summary>
    public decimal PresumidoEffectiveRate { get; init; }

    /// <summary>Rolling 12-month goods revenue used for Simples bracket (Anexo I).</summary>
    public decimal Rbt12Goods { get; init; }

    /// <summary>Rolling 12-month services revenue used for Simples bracket (Anexo III).</summary>
    public decimal Rbt12Services { get; init; }

    /// <summary>Lower estimated load for the sample period.</summary>
    public SuggestedTaxRegime SuggestedRegime { get; init; }

    /// <summary>Fixed disclaimer for accounting use.</summary>
    public string Disclaimer { get; init; } = RegimeSimulationResult.DefaultDisclaimer;

    /// <summary>Standard managerial disclaimer text.</summary>
    public const string DefaultDisclaimer =
        "Simulação gerencial com alíquotas simplificadas (LC 123 Anexos I/III — primeiras faixas; Presumido cumulativo). "
        + "Não substitui PGDAS, DCTF ou parecer contábil. ICMS no Presumido não estimado (emissão CSOSN 102).";
}

/// <summary>Combined fiscal planning output for API and export.</summary>
public sealed class FiscalPlanningReport
{
    /// <summary>Period apuration section.</summary>
    public FiscalPeriodReport Period { get; init; } = null!;

    /// <summary>Regime simulation section.</summary>
    public RegimeSimulation Simulation { get; init; } = null!;
}

/// <summary>Regime simulation wrapper including issuer ISS rate applied.</summary>
public sealed class RegimeSimulation
{
    /// <summary>Simulation numbers.</summary>
    public RegimeSimulationResult Result { get; init; } = null!;

    /// <summary>Issuer ISS rate (percent) used on services.</summary>
    public decimal IssRatePercent { get; init; }

    /// <summary>Issuer CRT / tax regime code (1 = Simples).</summary>
    public int TaxRegimeCode { get; init; }
}
