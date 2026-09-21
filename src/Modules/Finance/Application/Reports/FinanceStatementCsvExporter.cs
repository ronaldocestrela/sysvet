using System.Globalization;
using System.Text;
using Finance.Application.Reports.Dtos;

namespace Finance.Application.Reports;

/// <summary>
/// Builds UTF-8 CSV exports for monthly finance statements (cash flow + DRE sections).
/// </summary>
public static class FinanceStatementCsvExporter
{
    /// <summary>Serializes both report sections into a single CSV file with BOM.</summary>
    public static byte[] Export(CashFlowReportDto cashFlow, SimplifiedDreReportDto dre)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fluxo de caixa");
        sb.AppendLine("Data;Entrada;Saída;Líquido;Previsto receber;Previsto pagar");
        foreach (var day in cashFlow.Days)
        {
            sb.Append(day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            sb.Append(';');
            sb.Append(FormatDecimal(day.RealizedInflow));
            sb.Append(';');
            sb.Append(FormatDecimal(day.RealizedOutflow));
            sb.Append(';');
            sb.Append(FormatDecimal(day.NetRealized));
            sb.Append(';');
            sb.Append(FormatDecimal(day.ExpectedReceivable));
            sb.Append(';');
            sb.AppendLine(FormatDecimal(day.ExpectedPayable));
        }

        sb.AppendLine();
        sb.Append("Totais;;;");
        sb.Append(FormatDecimal(cashFlow.TotalRealizedInflow - cashFlow.TotalRealizedOutflow));
        sb.Append(';');
        sb.Append(FormatDecimal(cashFlow.TotalExpectedReceivable));
        sb.Append(';');
        sb.AppendLine(FormatDecimal(cashFlow.TotalExpectedPayable));

        sb.AppendLine();
        sb.AppendLine($"DRE simplificada;{dre.Year}-{dre.Month:D2}");
        sb.AppendLine("Categoria;Código;Receita;Despesa");
        foreach (var line in dre.Lines)
        {
            sb.Append(Escape(line.CategoryName));
            sb.Append(';');
            sb.Append(Escape(line.CategoryCode));
            sb.Append(';');
            sb.Append(FormatDecimal(line.Revenue));
            sb.Append(';');
            sb.AppendLine(FormatDecimal(line.Expense));
        }

        sb.Append("Total;;;");
        sb.Append(FormatDecimal(dre.TotalRevenue));
        sb.Append(';');
        sb.AppendLine(FormatDecimal(dre.TotalExpense));
        sb.Append("Resultado;;;");
        sb.Append(FormatDecimal(dre.NetResult));

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string FormatDecimal(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Escape(string value) =>
        value.Contains(';') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
