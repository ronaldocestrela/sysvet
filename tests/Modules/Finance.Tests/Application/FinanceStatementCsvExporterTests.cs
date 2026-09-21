using Finance.Application.Reports;
using Finance.Application.Reports.Dtos;
using FluentAssertions;

namespace Finance.Tests.Application;

public class FinanceStatementCsvExporterTests
{
    [Fact]
    public void Export_ShouldIncludeCashFlowAndDreSections()
    {
        var cashFlow = new CashFlowReportDto
        {
            From = new DateOnly(2026, 9, 1),
            To = new DateOnly(2026, 9, 30),
            Days =
            [
                new CashFlowDayDto
                {
                    Date = new DateOnly(2026, 9, 5),
                    RealizedInflow = 100m,
                    RealizedOutflow = 0m,
                    NetRealized = 100m
                }
            ],
            TotalRealizedInflow = 100m
        };

        var dre = new SimplifiedDreReportDto
        {
            Year = 2026,
            Month = 9,
            Lines =
            [
                new SimplifiedDreLineDto
                {
                    CategoryCode = "SALES",
                    CategoryName = "Vendas",
                    Revenue = 100m
                }
            ],
            TotalRevenue = 100m,
            NetResult = 100m
        };

        var bytes = FinanceStatementCsvExporter.Export(cashFlow, dre);
        var text = System.Text.Encoding.UTF8.GetString(bytes);

        text.Should().Contain("Fluxo de caixa");
        text.Should().Contain("DRE simplificada");
        text.Should().Contain("SALES");
        text.Should().Contain("2026-09-05");
    }
}
