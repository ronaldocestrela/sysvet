using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Models;
using Fiscal.Domain.Services;
using Fiscal.Domain.ValueObjects;
using FluentAssertions;

namespace Fiscal.Tests.Domain;

public class FiscalPlanningCalculatorTests
{
    private static readonly DateOnly MonthStart = new(2026, 9, 1);
    private static readonly DateOnly MonthEnd = new(2026, 9, 30);

    [Fact]
    public void ValidateDateRange_InvalidRange_ShouldFail()
    {
        var result = FiscalPlanningCalculator.ValidateDateRange(
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Fiscal.Report.InvalidDateRange");
    }

    [Fact]
    public void ValidateDateRange_Exceeds366Days_ShouldFail()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = from.AddDays(366);
        var result = FiscalPlanningCalculator.BuildPeriodReport(from, to, Array.Empty<FiscalDocument>(), 5m);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Fiscal.Report.RangeTooLarge");
    }

    [Fact]
    public void ValidateFullCalendarMonth_PartialMonth_ShouldFail()
    {
        var result = FiscalPlanningCalculator.ValidateFullCalendarMonth(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 15));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Fiscal.Report.ExportNotFullMonth");
    }

    [Fact]
    public void BuildPeriodReport_AuthorizedInPeriod_ShouldSum()
    {
        var nfe = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            100m,
            new DateTimeOffset(2026, 9, 5, 12, 0, 0, TimeSpan.Zero),
            cfop: "5102");

        var result = FiscalPlanningCalculator.BuildPeriodReport(MonthStart, MonthEnd, new[] { nfe }, 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.GoodsRevenue.Should().Be(100m);
        result.Value.ServicesRevenue.Should().Be(0m);
        result.Value.AuthorizedDocumentCount.Should().Be(1);
        result.Value.CfopBreakdown.Should().ContainSingle(l => l.Cfop == "5102" && l.GrossAmount == 100m);
    }

    [Fact]
    public void BuildPeriodReport_ContingencyAndRejected_ShouldExclude()
    {
        var contingency = CreateDraftNfce(50m);
        contingency.MarkContingencyIssued(
            "35250912345678901234656789012345678901234567",
            "xml",
            null,
            1,
            1,
            FiscalEmissionType.ContingencyOffline);

        var rejected = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            80m,
            new DateTimeOffset(2026, 9, 3, 12, 0, 0, TimeSpan.Zero));
        SetProperty(rejected, nameof(FiscalDocument.Status), FiscalDocumentStatus.Rejected);

        var result = FiscalPlanningCalculator.BuildPeriodReport(
            MonthStart,
            MonthEnd,
            new[] { contingency, rejected },
            5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalGrossRevenue.Should().Be(0m);
        result.Value.AuthorizedDocumentCount.Should().Be(0);
    }

    [Fact]
    public void BuildPeriodReport_CancelledInPeriod_ShouldEstorno()
    {
        var doc = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            120m,
            new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero));
        SetProperty(doc, nameof(FiscalDocument.Status), FiscalDocumentStatus.Cancelled);
        SetProperty(doc, nameof(FiscalDocument.CancelledAt), new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));

        var result = FiscalPlanningCalculator.BuildPeriodReport(MonthStart, MonthEnd, new[] { doc }, 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.GoodsRevenue.Should().Be(-120m);
        result.Value.CancelledDocumentCount.Should().Be(1);
    }

    [Fact]
    public void BuildPeriodReport_NfeAndNfce_AreGoods_Nfse_IsServices()
    {
        var nfe = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            40m,
            new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero));
        var nfce = CreateAuthorizedDocument(
            FiscalDocumentType.Nfce,
            30m,
            new DateTimeOffset(2026, 9, 3, 12, 0, 0, TimeSpan.Zero));
        var nfse = CreateAuthorizedNfse(70m, new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero));

        var result = FiscalPlanningCalculator.BuildPeriodReport(
            MonthStart,
            MonthEnd,
            new[] { nfe, nfce, nfse },
            5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.GoodsRevenue.Should().Be(70m);
        result.Value.ServicesRevenue.Should().Be(70m);
        result.Value.EstimatedIss.Should().Be(3.50m);
    }

    [Fact]
    public void BuildPeriodReport_Cfop5102And6102_ShouldBreakdown()
    {
        var intra = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            50m,
            new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero),
            cfop: "5102");
        var inter = CreateAuthorizedDocument(
            FiscalDocumentType.Nfe,
            80m,
            new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero),
            cfop: "6102");

        var result = FiscalPlanningCalculator.BuildPeriodReport(
            MonthStart,
            MonthEnd,
            new[] { intra, inter },
            5m);

        result.Value.CfopBreakdown.Should().HaveCount(2);
        result.Value.CfopBreakdown.Single(l => l.Cfop == "5102").GrossAmount.Should().Be(50m);
        result.Value.CfopBreakdown.Single(l => l.Cfop == "6102").GrossAmount.Should().Be(80m);
    }

    [Fact]
    public void BuildRegimeSimulation_ServiceClinicFirstBracket_SimplesShouldWin()
    {
        var result = FiscalPlanningCalculator.BuildRegimeSimulation(
            periodRevenueGoods: 0m,
            periodRevenueServices: 10_000m,
            rbt12Goods: 0m,
            rbt12Services: 120_000m,
            issRatePercent: 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.SimplesEstimatedTax.Should().Be(600m);
        result.Value.PresumidoEstimatedTax.Should().BeGreaterThan(600m);
        result.Value.SuggestedRegime.Should().Be(SuggestedTaxRegime.Simples);
    }

    [Fact]
    public void BuildRegimeSimulation_HighGoodsRbt12_PresumidoShouldWin()
    {
        var result = FiscalPlanningCalculator.BuildRegimeSimulation(
            periodRevenueGoods: 50_000m,
            periodRevenueServices: 0m,
            rbt12Goods: 400_000m,
            rbt12Services: 0m,
            issRatePercent: 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.SimplesEstimatedTax.Should().Be(50_000m * TaxRegimeTables.SimplesAnexoIGoodsThirdBracketRate);
        result.Value.PresumidoEstimatedTax.Should().BeLessThan(result.Value.SimplesEstimatedTax);
        result.Value.SuggestedRegime.Should().Be(SuggestedTaxRegime.Presumido);
    }

    [Fact]
    public void BuildPlanningReport_WithIssuer_ShouldCombinePeriodAndSimulation()
    {
        var issuer = CreateIssuer(defaultIssRate: 5m);
        var nfse = CreateAuthorizedNfse(1_000m, new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero));

        var result = FiscalPlanningCalculator.BuildPlanningReport(
            MonthStart,
            MonthEnd,
            new[] { nfse },
            issuer);

        result.IsSuccess.Should().BeTrue();
        result.Value.Period.ServicesRevenue.Should().Be(1_000m);
        result.Value.Simulation.TaxRegimeCode.Should().Be(1);
        result.Value.Simulation.Result.Disclaimer.Should().Contain("PGDAS");
    }

    private static IssuerProfile CreateIssuer(decimal defaultIssRate)
    {
        var cnpj = FiscalCnpj.Create("12345678000195").Value;
        return IssuerProfile.Create(
            "Clinic LTDA",
            "Clinic",
            cnpj,
            "123",
            "456",
            "7500100",
            "Rua A",
            "1",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001000",
            3550308,
            "11999999999",
            "0107",
            defaultIssRate,
            FiscalEnvironment.Homologation).Value;
    }

    private static FiscalDocument CreateAuthorizedDocument(
        FiscalDocumentType type,
        decimal total,
        DateTimeOffset authorizedAt,
        string cfop = "5102")
    {
        var doc = type == FiscalDocumentType.Nfce
            ? CreateDraftNfce(total)
            : FiscalDocument.CreateDraft(type, Guid.NewGuid(), "Cliente", null, "SP").Value;

        if (type != FiscalDocumentType.Nfce)
        {
            doc.AddProductItem(Guid.NewGuid(), "Produto", 1, total, "23091000", cfop, "102", 0);
        }

        doc.BeginTransmission();
        doc.MarkAuthorized(
            type == FiscalDocumentType.Nfce
                ? "35250912345678901234656789012345678901234567"
                : "35250912345678901234567890123456789012345678",
            "PROT",
            "xml",
            null,
            1,
            1,
            null);
        SetProperty(doc, nameof(FiscalDocument.AuthorizedAt), authorizedAt);
        return doc;
    }

    private static FiscalDocument CreateDraftNfce(decimal total)
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfce, Guid.NewGuid(), "Consumidor", null, "SP").Value;
        doc.AddProductItem(Guid.NewGuid(), "Item", 1, total, "23091000", "5102", "102", 0);
        return doc;
    }

    private static FiscalDocument CreateAuthorizedNfse(decimal total, DateTimeOffset authorizedAt)
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfse, Guid.NewGuid(), "Cliente", null, "SP").Value;
        doc.AddServiceItem("Consulta", 1, total);
        doc.BeginTransmission();
        doc.MarkAuthorized(null, "PROT", "xml", null, null, null, "123");
        SetProperty(doc, nameof(FiscalDocument.AuthorizedAt), authorizedAt);
        return doc;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        typeof(FiscalDocument).GetProperty(propertyName)!
            .SetValue(target, value);
    }
}
