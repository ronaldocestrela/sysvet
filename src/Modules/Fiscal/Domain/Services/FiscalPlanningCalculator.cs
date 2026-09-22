using Core.Domain;
using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Models;

namespace Fiscal.Domain.Services;

/// <summary>
/// Builds period tax apuration and managerial regime simulation from fiscal documents.
/// </summary>
public static class FiscalPlanningCalculator
{
    private const int MaxReportDays = 366;
    private const decimal RateComparisonTolerance = 0.0001m;

    /// <summary>Validates an inclusive date range for reports.</summary>
    public static Result ValidateDateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            return Result.Failure(ErrorCodes.Report.InvalidDateRange);
        }

        var dayCount = to.DayNumber - from.DayNumber + 1;
        if (dayCount > MaxReportDays)
        {
            return Result.Failure(ErrorCodes.Report.RangeTooLarge);
        }

        return Result.Success();
    }

    /// <summary>Requires <paramref name="from"/>..<paramref name="to"/> to be one full calendar month.</summary>
    public static Result ValidateFullCalendarMonth(DateOnly from, DateOnly to)
    {
        var range = ValidateDateRange(from, to);
        if (range.IsFailure)
        {
            return range;
        }

        if (from.Day != 1 || to != from.AddMonths(1).AddDays(-1))
        {
            return Result.Failure(ErrorCodes.Report.ExportNotFullMonth);
        }

        return Result.Success();
    }

    /// <summary>
    /// Builds period apuration and regime simulation for the tenant issuer.
    /// </summary>
    public static Result<FiscalPlanningReport> BuildPlanningReport(
        DateOnly from,
        DateOnly to,
        IReadOnlyList<FiscalDocument> documentsForPeriodAndRbt12,
        IssuerProfile issuer)
    {
        var range = ValidateDateRange(from, to);
        if (range.IsFailure)
        {
            return Result.Failure<FiscalPlanningReport>(range.Error);
        }

        var periodResult = BuildPeriodReport(from, to, documentsForPeriodAndRbt12, issuer.DefaultIssRate);
        if (periodResult.IsFailure)
        {
            return Result.Failure<FiscalPlanningReport>(periodResult.Error);
        }

        var rbt12Start = GetRbt12Start(to);
        var (rbt12Goods, rbt12Services) = ComputeRbt12(documentsForPeriodAndRbt12, rbt12Start, to);

        var simulationResult = BuildRegimeSimulation(
            periodResult.Value.GoodsRevenue,
            periodResult.Value.ServicesRevenue,
            rbt12Goods,
            rbt12Services,
            issuer.DefaultIssRate);

        if (simulationResult.IsFailure)
        {
            return Result.Failure<FiscalPlanningReport>(simulationResult.Error);
        }

        return Result.Success(new FiscalPlanningReport
        {
            Period = periodResult.Value,
            Simulation = new RegimeSimulation
            {
                Result = simulationResult.Value,
                IssRatePercent = issuer.DefaultIssRate,
                TaxRegimeCode = issuer.TaxRegimeCode
            }
        });
    }

    /// <summary>Aggregates net revenue and CFOP breakdown for authorized/cancelled documents in the period.</summary>
    public static Result<FiscalPeriodReport> BuildPeriodReport(
        DateOnly from,
        DateOnly to,
        IReadOnlyList<FiscalDocument> documents,
        decimal issRatePercent)
    {
        var range = ValidateDateRange(from, to);
        if (range.IsFailure)
        {
            return Result.Failure<FiscalPeriodReport>(range.Error);
        }

        decimal goods = 0;
        decimal services = 0;
        var authorizedCount = 0;
        var cancelledCount = 0;
        var cfopTotals = new Dictionary<string, (decimal Amount, int Count)>();

        foreach (var doc in documents)
        {
            if (doc.Status == FiscalDocumentStatus.Authorized && IsDateInRange(doc.AuthorizedAt, from, to))
            {
                ApplyDocumentRevenue(doc, 1, ref goods, ref services, cfopTotals);
                authorizedCount++;
            }
            else if (doc.Status == FiscalDocumentStatus.Cancelled && IsDateInRange(doc.CancelledAt, from, to))
            {
                ApplyDocumentRevenue(doc, -1, ref goods, ref services, cfopTotals);
                cancelledCount++;
            }
            else if (IsExcludedFromPeriod(doc))
            {
                continue;
            }
        }

        var cfopLines = cfopTotals
            .OrderBy(kv => kv.Key)
            .Select(kv => new CfopBreakdownLine(kv.Key, kv.Value.Amount, kv.Value.Count))
            .ToList();

        var estimatedIss = services > 0 && issRatePercent > 0
            ? Math.Round(services * issRatePercent / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return Result.Success(new FiscalPeriodReport
        {
            From = from,
            To = to,
            GoodsRevenue = goods,
            ServicesRevenue = services,
            EstimatedIss = estimatedIss,
            AuthorizedDocumentCount = authorizedCount,
            CancelledDocumentCount = cancelledCount,
            CfopBreakdown = cfopLines
        });
    }

    /// <summary>Compares estimated Simples DAS vs Lucro Presumido for sample period revenue.</summary>
    public static Result<RegimeSimulationResult> BuildRegimeSimulation(
        decimal periodRevenueGoods,
        decimal periodRevenueServices,
        decimal rbt12Goods,
        decimal rbt12Services,
        decimal issRatePercent)
    {
        var goodsRate = TaxRegimeTables.ResolveSimplesGoodsRate(rbt12Goods);
        var servicesRate = TaxRegimeTables.ResolveSimplesServicesRate(rbt12Services);

        var simplesTax = Math.Round(
            periodRevenueGoods * goodsRate + periodRevenueServices * servicesRate,
            2,
            MidpointRounding.AwayFromZero);

        var presumidoTax = Math.Round(
            EstimatePresumidoTax(periodRevenueGoods, periodRevenueServices, issRatePercent),
            2,
            MidpointRounding.AwayFromZero);

        var totalRevenue = periodRevenueGoods + periodRevenueServices;
        var simplesEffective = totalRevenue > 0 ? simplesTax / totalRevenue : 0m;
        var presumidoEffective = totalRevenue > 0 ? presumidoTax / totalRevenue : 0m;

        var suggested = SuggestedTaxRegime.Tie;
        if (simplesTax + RateComparisonTolerance < presumidoTax)
        {
            suggested = SuggestedTaxRegime.Simples;
        }
        else if (presumidoTax + RateComparisonTolerance < simplesTax)
        {
            suggested = SuggestedTaxRegime.Presumido;
        }

        return Result.Success(new RegimeSimulationResult
        {
            SimplesEstimatedTax = simplesTax,
            PresumidoEstimatedTax = presumidoTax,
            SimplesEffectiveRate = simplesEffective,
            PresumidoEffectiveRate = presumidoEffective,
            Rbt12Goods = rbt12Goods,
            Rbt12Services = rbt12Services,
            SuggestedRegime = suggested
        });
    }

    /// <summary>First day of the rolling 12-month window ending at <paramref name="to"/>.</summary>
    public static DateOnly GetRbt12Start(DateOnly to) =>
        new DateOnly(to.Year, to.Month, 1).AddMonths(-11);

    private static decimal EstimatePresumidoTax(decimal goods, decimal services, decimal issRatePercent)
    {
        var goodsFederal = goods * (
            TaxRegimeTables.PresumidoGoodsIrpjBaseRate * TaxRegimeTables.PresumidoIrpjRate
            + TaxRegimeTables.PresumidoGoodsCsllBaseRate * TaxRegimeTables.PresumidoCsllRate
            + TaxRegimeTables.PresumidoPisRate
            + TaxRegimeTables.PresumidoCofinsRate);

        var servicesFederal = services * (
            TaxRegimeTables.PresumidoServicesBaseRate * TaxRegimeTables.PresumidoIrpjRate
            + TaxRegimeTables.PresumidoServicesBaseRate * TaxRegimeTables.PresumidoCsllRate
            + TaxRegimeTables.PresumidoPisRate
            + TaxRegimeTables.PresumidoCofinsRate);

        var iss = issRatePercent > 0 ? services * issRatePercent / 100m : 0m;
        return goodsFederal + servicesFederal + iss;
    }

    private static (decimal Goods, decimal Services) ComputeRbt12(
        IReadOnlyList<FiscalDocument> documents,
        DateOnly rbt12Start,
        DateOnly to)
    {
        decimal goods = 0;
        decimal services = 0;

        foreach (var doc in documents)
        {
            if (doc.AuthorizedAt is null)
            {
                continue;
            }

            var authDate = DateOnly.FromDateTime(doc.AuthorizedAt.Value.UtcDateTime);
            if (authDate < rbt12Start || authDate > to)
            {
                continue;
            }

            if (doc.DocumentType is FiscalDocumentType.Nfe or FiscalDocumentType.Nfce)
            {
                goods += doc.TotalAmount;
            }
            else if (doc.DocumentType == FiscalDocumentType.Nfse)
            {
                services += doc.TotalAmount;
            }
        }

        return (goods, services);
    }

    private static void ApplyDocumentRevenue(
        FiscalDocument doc,
        int sign,
        ref decimal goods,
        ref decimal services,
        Dictionary<string, (decimal Amount, int Count)> cfopTotals)
    {
        var amount = doc.TotalAmount * sign;
        if (doc.DocumentType is FiscalDocumentType.Nfe or FiscalDocumentType.Nfce)
        {
            goods += amount;
            foreach (var line in doc.Items.Where(i => !string.IsNullOrWhiteSpace(i.Cfop)))
            {
                var cfop = line.Cfop!;
                var lineTotal = line.Total * sign;
                if (!cfopTotals.TryGetValue(cfop, out var bucket))
                {
                    bucket = (0, 0);
                }

                cfopTotals[cfop] = (bucket.Amount + lineTotal, bucket.Count + (sign > 0 ? 1 : 0));
            }
        }
        else if (doc.DocumentType == FiscalDocumentType.Nfse)
        {
            services += amount;
        }
    }

    private static bool IsExcludedFromPeriod(FiscalDocument doc) =>
        doc.Status is FiscalDocumentStatus.ContingencyIssued
            or FiscalDocumentStatus.Rejected
            or FiscalDocumentStatus.Denied
            or FiscalDocumentStatus.Draft
            or FiscalDocumentStatus.Transmitting;

    private static bool IsDateInRange(DateTimeOffset? instant, DateOnly from, DateOnly to)
    {
        if (instant is null)
        {
            return false;
        }

        var date = DateOnly.FromDateTime(instant.Value.UtcDateTime);
        return date >= from && date <= to;
    }
}
