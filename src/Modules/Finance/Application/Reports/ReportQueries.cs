using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.Reports.Dtos;

namespace Finance.Application.Reports;

/// <summary>Supported export formats for monthly finance statements.</summary>
public enum FinanceExportFormat
{
    Csv,
    Pdf
}

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record GetCashFlowQuery(DateOnly From, DateOnly To) : IQuery<CashFlowReportDto>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record GetSimplifiedDreQuery(int Year, int Month) : IQuery<SimplifiedDreReportDto>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record ExportFinanceStatementsQuery(DateOnly From, DateOnly To, FinanceExportFormat Format) : IQuery<ReportFileDto>;

/// <summary>Binary report file returned by export queries.</summary>
public sealed record ReportFileDto(byte[] Content, string ContentType, string FileName);
