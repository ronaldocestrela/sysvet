namespace Fiscal.Application.Planning.Dtos;

/// <summary>CFOP line in period tax report.</summary>
public sealed record CfopBreakdownLineDto(string Cfop, decimal GrossAmount, int DocumentCount);

/// <summary>Period tax apuration section.</summary>
public sealed record FiscalPeriodReportDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public decimal GoodsRevenue { get; init; }
    public decimal ServicesRevenue { get; init; }
    public decimal TotalGrossRevenue { get; init; }
    public decimal EstimatedIss { get; init; }
    public int AuthorizedDocumentCount { get; init; }
    public int CancelledDocumentCount { get; init; }
    public IReadOnlyList<CfopBreakdownLineDto> CfopBreakdown { get; init; } = Array.Empty<CfopBreakdownLineDto>();
}

/// <summary>Simples vs Presumido managerial simulation section.</summary>
public sealed record RegimeSimulationDto
{
    public decimal SimplesEstimatedTax { get; init; }
    public decimal PresumidoEstimatedTax { get; init; }
    public decimal SimplesEffectiveRate { get; init; }
    public decimal PresumidoEffectiveRate { get; init; }
    public decimal Rbt12Goods { get; init; }
    public decimal Rbt12Services { get; init; }
    public string SuggestedRegime { get; init; } = string.Empty;
    public string Disclaimer { get; init; } = string.Empty;
    public decimal IssRatePercent { get; init; }
    public int TaxRegimeCode { get; init; }
}

/// <summary>Combined fiscal planning API response.</summary>
public sealed record FiscalPlanningReportDto(FiscalPeriodReportDto Period, RegimeSimulationDto Simulation);
