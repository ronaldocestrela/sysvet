using Fiscal.Application.Planning.Dtos;

namespace Fiscal.Application.Planning;

/// <summary>Renders fiscal planning report sections to PDF bytes.</summary>
public interface IFiscalPlanningPdfRenderer
{
    /// <summary>Builds a single PDF with period apuration and regime simulation.</summary>
    byte[] Render(FiscalPlanningReportDto report);
}
