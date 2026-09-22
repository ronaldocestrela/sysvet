using Core.Application.Messaging;
using Core.Domain;
using Fiscal.Application.Planning.Dtos;
using Fiscal.Domain.Repositories;
using Fiscal.Domain.Services;
using MediatR;

namespace Fiscal.Application.Planning;

/// <summary>Handles fiscal planning read queries.</summary>
public sealed class GetFiscalPlanningQueryHandler : IRequestHandler<GetFiscalPlanningQuery, Result<FiscalPlanningReportDto>>
{
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly IIssuerProfileRepository _issuerRepository;

    public GetFiscalPlanningQueryHandler(
        IFiscalDocumentRepository documentRepository,
        IIssuerProfileRepository issuerRepository)
    {
        _documentRepository = documentRepository;
        _issuerRepository = issuerRepository;
    }

    public async Task<Result<FiscalPlanningReportDto>> Handle(GetFiscalPlanningQuery request, CancellationToken cancellationToken)
    {
        var build = await BuildReportAsync(request.From, request.To, cancellationToken);
        return build.IsFailure
            ? Result.Failure<FiscalPlanningReportDto>(build.Error)
            : Result.Success(FiscalPlanningMapper.ToDto(build.Value));
    }

    internal static async Task<Result<Fiscal.Domain.Models.FiscalPlanningReport>> BuildReportAsync(
        DateOnly from,
        DateOnly to,
        IFiscalDocumentRepository documentRepository,
        IIssuerProfileRepository issuerRepository,
        CancellationToken cancellationToken)
    {
        var issuer = await issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Failure<Fiscal.Domain.Models.FiscalPlanningReport>(Fiscal.Domain.ErrorCodes.Issuer.NotFound);
        }

        var documents = await documentRepository.ListForPlanningAsync(from, to, cancellationToken);
        return FiscalPlanningCalculator.BuildPlanningReport(from, to, documents, issuer);
    }

    private async Task<Result<Fiscal.Domain.Models.FiscalPlanningReport>> BuildReportAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken) =>
        await BuildReportAsync(from, to, _documentRepository, _issuerRepository, cancellationToken);
}

/// <summary>Exports fiscal planning as CSV or PDF.</summary>
public sealed class ExportFiscalPlanningQueryHandler : IRequestHandler<ExportFiscalPlanningQuery, Result<FiscalPlanningReportFileDto>>
{
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalPlanningPdfRenderer _pdfRenderer;

    public ExportFiscalPlanningQueryHandler(
        IFiscalDocumentRepository documentRepository,
        IIssuerProfileRepository issuerRepository,
        IFiscalPlanningPdfRenderer pdfRenderer)
    {
        _documentRepository = documentRepository;
        _issuerRepository = issuerRepository;
        _pdfRenderer = pdfRenderer;
    }

    public async Task<Result<FiscalPlanningReportFileDto>> Handle(ExportFiscalPlanningQuery request, CancellationToken cancellationToken)
    {
        var monthValidation = FiscalPlanningCalculator.ValidateFullCalendarMonth(request.From, request.To);
        if (monthValidation.IsFailure)
        {
            return Result.Failure<FiscalPlanningReportFileDto>(monthValidation.Error);
        }

        var build = await GetFiscalPlanningQueryHandler.BuildReportAsync(
            request.From,
            request.To,
            _documentRepository,
            _issuerRepository,
            cancellationToken);
        if (build.IsFailure)
        {
            return Result.Failure<FiscalPlanningReportFileDto>(build.Error);
        }

        var dto = FiscalPlanningMapper.ToDto(build.Value);
        var extension = request.Format == FiscalExportFormat.Csv ? "csv" : "pdf";
        var fileName = $"fiscal-planning-{request.From:yyyy-MM}.{extension}";

        return request.Format switch
        {
            FiscalExportFormat.Csv => Result.Success(new FiscalPlanningReportFileDto(
                FiscalPlanningCsvExporter.Export(dto),
                "text/csv",
                fileName)),
            FiscalExportFormat.Pdf => Result.Success(new FiscalPlanningReportFileDto(
                _pdfRenderer.Render(dto),
                "application/pdf",
                fileName)),
            _ => Result.Failure<FiscalPlanningReportFileDto>(Fiscal.Domain.ErrorCodes.Report.ExportNotFullMonth)
        };
    }
}
