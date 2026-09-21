using Finance.Application.Reports;
using Finance.Application.Reports.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finance.Infrastructure.Reports;

/// <summary>
/// Renders monthly cash-flow and DRE sections to a single PDF using QuestPDF Community license.
/// </summary>
public sealed class QuestPdfFinanceStatementRenderer : IFinanceStatementPdfRenderer
{
    static QuestPdfFinanceStatementRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public byte[] Render(CashFlowReportDto cashFlow, SimplifiedDreReportDto dre)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Content().Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text("Demonstrativos financeiros").FontSize(16).Bold();
                    column.Item().Text($"Período: {cashFlow.From:yyyy-MM-dd} — {cashFlow.To:yyyy-MM-dd}");

                    column.Item().PaddingTop(10).Text("Fluxo de caixa (regime de caixa)").Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Data");
                            header.Cell().Text("Entrada");
                            header.Cell().Text("Saída");
                            header.Cell().Text("Líquido");
                        });

                        foreach (var day in cashFlow.Days.Where(d =>
                                     d.RealizedInflow != 0 || d.RealizedOutflow != 0
                                     || d.ExpectedReceivable != 0 || d.ExpectedPayable != 0))
                        {
                            table.Cell().Text(day.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Text(day.RealizedInflow.ToString("N2"));
                            table.Cell().Text(day.RealizedOutflow.ToString("N2"));
                            table.Cell().Text(day.NetRealized.ToString("N2"));
                        }
                    });

                    column.Item().Text(
                        $"Totais — Entrada: {cashFlow.TotalRealizedInflow:N2} | Saída: {cashFlow.TotalRealizedOutflow:N2}");

                    column.Item().PaddingTop(10).Text($"DRE simplificada ({dre.Year}-{dre.Month:D2})").Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Categoria");
                            header.Cell().Text("Receita");
                            header.Cell().Text("Despesa");
                        });

                        foreach (var line in dre.Lines)
                        {
                            table.Cell().Text($"{line.CategoryName} ({line.CategoryCode})");
                            table.Cell().Text(line.Revenue.ToString("N2"));
                            table.Cell().Text(line.Expense.ToString("N2"));
                        }
                    });

                    column.Item().Text(
                        $"Resultado: {dre.NetResult:N2} (Receita {dre.TotalRevenue:N2} − Despesa {dre.TotalExpense:N2})");
                });
            });
        });

        return document.GeneratePdf();
    }
}
