using Finance.Application.Reports.Dtos;

namespace Finance.Application.Reports;

/// <summary>
/// Renders combined cash-flow and DRE reports to PDF (implemented in Infrastructure).
/// </summary>
public interface IFinanceStatementPdfRenderer
{
    /// <summary>Builds a PDF document for the given statement DTOs.</summary>
    byte[] Render(CashFlowReportDto cashFlow, SimplifiedDreReportDto dre);
}
