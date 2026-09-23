using System.Globalization;
using System.Text;
using Intelligence.Application.Reports.Dtos;

namespace Intelligence.Application.Reports;

/// <summary>UTF-8 CSV exports for intelligence strategic reports (10.3).</summary>
public static class IntelligenceReportCsvExporter
{
    /// <summary>Exports ABC customer rows.</summary>
    public static byte[] ExportAbcCustomers(AbcCustomerReportDto report) =>
        Export(
            "Ranking;Cliente;Valor líquido;Participação %;Acumulado %;Classe",
            report.Rows.Select(r =>
                $"{r.Rank};{Escape(r.DisplayName)};{Format(r.NetAmount)};{Format(r.SharePercent)};{Format(r.CumulativeSharePercent)};{r.Class}"));

    /// <summary>Exports ABC product rows.</summary>
    public static byte[] ExportAbcProducts(AbcProductReportDto report) =>
        Export(
            "Ranking;Produto;Valor líquido;Quantidade;Participação %;Acumulado %;Classe",
            report.Rows.Select(r =>
                $"{r.Rank};{Escape(r.ProductName)};{Format(r.NetAmount)};{Format(r.NetQuantity)};{Format(r.SharePercent)};{Format(r.CumulativeSharePercent)};{r.Class}"));

    /// <summary>Exports productivity rows.</summary>
    public static byte[] ExportProductivity(ProductivityReportDto report) =>
        Export(
            "Profissional;Receita PDV;Quantidade PDV;Consultas concluídas;Banho concluído",
            report.Rows.Select(r =>
                $"{Escape(r.DisplayName)};{Format(r.SalesNetAmount)};{Format(r.SalesQuantity)};{r.ClinicalCompleted};{r.GroomingCompleted}"));

    private static byte[] Export(string header, IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var line in lines)
        {
            sb.AppendLine(line);
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Format(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Escape(string value) =>
        value.Contains(';') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
}
