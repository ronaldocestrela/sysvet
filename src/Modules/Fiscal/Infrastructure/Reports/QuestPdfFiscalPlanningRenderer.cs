using Fiscal.Application.Planning;
using Fiscal.Application.Planning.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Fiscal.Infrastructure.Reports;

/// <summary>Renders fiscal planning period and simulation sections to PDF.</summary>
public sealed class QuestPdfFiscalPlanningRenderer : IFiscalPlanningPdfRenderer
{
    static QuestPdfFiscalPlanningRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public byte[] Render(FiscalPlanningReportDto report)
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
                    column.Item().Text("Planejamento fiscal").FontSize(16).Bold();
                    column.Item().Text($"Período: {report.Period.From:yyyy-MM-dd} — {report.Period.To:yyyy-MM-dd}");

                    column.Item().PaddingTop(10).Text("Apuração do período").Bold();
                    column.Item().Text($"Mercadorias (NF-e/NFC-e): {report.Period.GoodsRevenue:N2}");
                    column.Item().Text($"Serviços (NFS-e): {report.Period.ServicesRevenue:N2}");
                    column.Item().Text($"Receita bruta: {report.Period.TotalGrossRevenue:N2}");
                    column.Item().Text($"ISS estimado: {report.Period.EstimatedIss:N2}");
                    column.Item().Text(
                        $"Documentos: {report.Period.AuthorizedDocumentCount} autorizadas, {report.Period.CancelledDocumentCount} cancelamentos (estorno)");

                    if (report.Period.CfopBreakdown.Count > 0)
                    {
                        column.Item().PaddingTop(6).Text("CFOP").Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("CFOP");
                                header.Cell().Text("Valor");
                                header.Cell().Text("Docs");
                            });
                            foreach (var line in report.Period.CfopBreakdown)
                            {
                                table.Cell().Text(line.Cfop);
                                table.Cell().Text(line.GrossAmount.ToString("N2"));
                                table.Cell().Text(line.DocumentCount.ToString());
                            }
                        });
                    }

                    column.Item().PaddingTop(10).Text("Simulação de enquadramento").Bold();
                    column.Item().Text($"Simples Nacional (estimado): {report.Simulation.SimplesEstimatedTax:N2} ({report.Simulation.SimplesEffectiveRate:P2})");
                    column.Item().Text($"Lucro Presumido (estimado): {report.Simulation.PresumidoEstimatedTax:N2} ({report.Simulation.PresumidoEffectiveRate:P2})");
                    column.Item().Text($"RBT12 mercadorias: {report.Simulation.Rbt12Goods:N2}");
                    column.Item().Text($"RBT12 serviços: {report.Simulation.Rbt12Services:N2}");
                    column.Item().Text($"Sugestão gerencial: {report.Simulation.SuggestedRegime}");
                    column.Item().PaddingTop(8).Text(report.Simulation.Disclaimer).FontSize(8).Italic();
                });
            });
        });

        return document.GeneratePdf();
    }
}
