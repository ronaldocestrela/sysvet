using System.Globalization;
using System.Text;
using Core.Domain.Entitlements;

namespace Platform.Application.Metrics;

/// <summary>CSV export for module adoption heatmap (10.3).</summary>
public static class ModuleAdoptionCsvExporter
{
    /// <summary>Serializes tenant × module matrix with UTF-8 BOM.</summary>
    public static byte[] Export(PlatformModuleAdoptionDto adoption)
    {
        var modules = Enum.GetValues<CommercialModule>().OrderBy(m => (int)m).ToList();
        var sb = new StringBuilder();
        sb.Append("Tenant");
        foreach (var module in modules)
        {
            sb.Append(';');
            sb.Append(module);
        }

        sb.AppendLine();

        foreach (var tenant in adoption.Tenants)
        {
            sb.Append(Escape(tenant.DisplayName));
            var cellMap = tenant.Cells.ToDictionary(c => c.Module, c => c.Enabled);
            foreach (var module in modules)
            {
                sb.Append(';');
                sb.Append(cellMap.TryGetValue(module, out var enabled) && enabled ? "1" : "0");
            }

            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Resumo;Habilitados;Taxa");
        foreach (var summary in adoption.ModuleSummaries)
        {
            sb.Append(summary.Module);
            sb.Append(';');
            sb.Append(summary.EnabledCount.ToString(CultureInfo.InvariantCulture));
            sb.Append(';');
            sb.AppendLine(summary.AdoptionRate.ToString("0.0000", CultureInfo.InvariantCulture));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Escape(string value) =>
        value.Contains(';') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
}
