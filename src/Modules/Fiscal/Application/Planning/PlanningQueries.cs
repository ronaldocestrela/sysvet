using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Fiscal.Application.Planning.Dtos;

namespace Fiscal.Application.Planning;

/// <summary>Supported export formats for fiscal planning reports.</summary>
public enum FiscalExportFormat
{
    Csv,
    Pdf
}

/// <summary>Loads period apuration and regime simulation for the tenant.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record GetFiscalPlanningQuery(DateOnly From, DateOnly To) : IQuery<FiscalPlanningReportDto>;

/// <summary>Exports fiscal planning report as CSV or PDF for accounting handoff.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record ExportFiscalPlanningQuery(DateOnly From, DateOnly To, FiscalExportFormat Format) : IQuery<FiscalPlanningReportFileDto>;

/// <summary>Binary fiscal planning file returned by export queries.</summary>
public sealed record FiscalPlanningReportFileDto(byte[] Content, string ContentType, string FileName);
