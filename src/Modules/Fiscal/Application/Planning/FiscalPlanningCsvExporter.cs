using System.Globalization;
using System.Text;
using Fiscal.Application.Planning.Dtos;

namespace Fiscal.Application.Planning;

/// <summary>Builds UTF-8 CSV exports for fiscal planning (period + simulation).</summary>
public static class FiscalPlanningCsvExporter
{
    /// <summary>Serializes planning sections into a single CSV file with BOM.</summary>
    public static byte[] Export(FiscalPlanningReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Apuração do período");
        sb.AppendLine($"De;{report.Period.From:yyyy-MM-dd};Até;{report.Period.To:yyyy-MM-dd}");
        sb.AppendLine("Receita mercadorias (NF-e/NFC-e);Receita serviços (NFS-e);Receita bruta;ISS estimado;Autorizadas;Cancelamentos (estorno)");
        sb.Append(FormatDecimal(report.Period.GoodsRevenue));
        sb.Append(';');
        sb.Append(FormatDecimal(report.Period.ServicesRevenue));
        sb.Append(';');
        sb.Append(FormatDecimal(report.Period.TotalGrossRevenue));
        sb.Append(';');
        sb.Append(FormatDecimal(report.Period.EstimatedIss));
        sb.Append(';');
        sb.Append(report.Period.AuthorizedDocumentCount);
        sb.Append(';');
        sb.AppendLine(report.Period.CancelledDocumentCount.ToString(CultureInfo.InvariantCulture));

        sb.AppendLine();
        sb.AppendLine("CFOP;Valor;Documentos");
        foreach (var line in report.Period.CfopBreakdown)
        {
            sb.Append(Escape(line.Cfop));
            sb.Append(';');
            sb.Append(FormatDecimal(line.GrossAmount));
            sb.Append(';');
            sb.AppendLine(line.DocumentCount.ToString(CultureInfo.InvariantCulture));
        }

        sb.AppendLine();
        sb.AppendLine("Simulação de enquadramento");
        sb.AppendLine("Regime;Imposto estimado no período;Alíquota efetiva");
        sb.Append("Simples Nacional;");
        sb.Append(FormatDecimal(report.Simulation.SimplesEstimatedTax));
        sb.Append(';');
        sb.AppendLine(FormatPercent(report.Simulation.SimplesEffectiveRate));
        sb.Append("Lucro Presumido;");
        sb.Append(FormatDecimal(report.Simulation.PresumidoEstimatedTax));
        sb.Append(';');
        sb.AppendLine(FormatPercent(report.Simulation.PresumidoEffectiveRate));
        sb.AppendLine($"RBT12 mercadorias;{FormatDecimal(report.Simulation.Rbt12Goods)}");
        sb.AppendLine($"RBT12 serviços;{FormatDecimal(report.Simulation.Rbt12Services)}");
        sb.AppendLine($"Sugestão;{Escape(report.Simulation.SuggestedRegime)}");
        sb.AppendLine($"CRT emitente;{report.Simulation.TaxRegimeCode}");
        sb.AppendLine();
        sb.AppendLine(Escape(report.Simulation.Disclaimer));

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string FormatDecimal(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string FormatPercent(decimal rate) =>
        (rate * 100m).ToString("0.00", CultureInfo.InvariantCulture) + "%";

    private static string Escape(string value) =>
        value.Contains(';') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
