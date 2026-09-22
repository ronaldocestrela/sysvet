using Fiscal.Application.Planning.Dtos;
using Fiscal.Domain.Models;

namespace Fiscal.Application.Planning;

/// <summary>Maps domain planning models to API DTOs.</summary>
public static class FiscalPlanningMapper
{
    /// <summary>Maps a full planning report to DTO.</summary>
    public static FiscalPlanningReportDto ToDto(FiscalPlanningReport report) =>
        new(ToDto(report.Period), ToDto(report.Simulation));

    private static FiscalPeriodReportDto ToDto(FiscalPeriodReport period) =>
        new()
        {
            From = period.From,
            To = period.To,
            GoodsRevenue = period.GoodsRevenue,
            ServicesRevenue = period.ServicesRevenue,
            TotalGrossRevenue = period.TotalGrossRevenue,
            EstimatedIss = period.EstimatedIss,
            AuthorizedDocumentCount = period.AuthorizedDocumentCount,
            CancelledDocumentCount = period.CancelledDocumentCount,
            CfopBreakdown = period.CfopBreakdown
                .Select(l => new CfopBreakdownLineDto(l.Cfop, l.GrossAmount, l.DocumentCount))
                .ToList()
        };

    private static RegimeSimulationDto ToDto(RegimeSimulation simulation) =>
        new()
        {
            SimplesEstimatedTax = simulation.Result.SimplesEstimatedTax,
            PresumidoEstimatedTax = simulation.Result.PresumidoEstimatedTax,
            SimplesEffectiveRate = simulation.Result.SimplesEffectiveRate,
            PresumidoEffectiveRate = simulation.Result.PresumidoEffectiveRate,
            Rbt12Goods = simulation.Result.Rbt12Goods,
            Rbt12Services = simulation.Result.Rbt12Services,
            SuggestedRegime = simulation.Result.SuggestedRegime.ToString(),
            Disclaimer = simulation.Result.Disclaimer,
            IssRatePercent = simulation.IssRatePercent,
            TaxRegimeCode = simulation.TaxRegimeCode
        };
}
