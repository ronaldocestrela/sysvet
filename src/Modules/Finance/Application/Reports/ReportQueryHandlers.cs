using Core.Application.Messaging;
using Core.Domain;
using Finance.Application.Reports.Dtos;
using Finance.Domain;
using Finance.Domain.Repositories;
using Finance.Domain.Services;
using MediatR;

namespace Finance.Application.Reports;

public sealed class GetCashFlowQueryHandler : IRequestHandler<GetCashFlowQuery, Result<CashFlowReportDto>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public GetCashFlowQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<CashFlowReportDto>> Handle(GetCashFlowQuery request, CancellationToken cancellationToken)
    {
        var titles = await _titleRepository.ListForStatementsAsync(request.From, request.To, cancellationToken);
        var statement = FinancialStatementCalculator.BuildCashFlow(request.From, request.To, titles);
        if (statement.IsFailure)
        {
            return Result.Failure<CashFlowReportDto>(statement.Error);
        }

        return Result.Success(FinanceReportMapper.ToDto(statement.Value));
    }
}

public sealed class GetSimplifiedDreQueryHandler : IRequestHandler<GetSimplifiedDreQuery, Result<SimplifiedDreReportDto>>
{
    private readonly IFinancialTitleRepository _titleRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;

    public GetSimplifiedDreQueryHandler(
        IFinancialTitleRepository titleRepository,
        IFinancialCategoryRepository categoryRepository)
    {
        _titleRepository = titleRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<SimplifiedDreReportDto>> Handle(GetSimplifiedDreQuery request, CancellationToken cancellationToken)
    {
        var from = new DateOnly(request.Year, request.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var titles = await _titleRepository.ListForStatementsAsync(from, to, cancellationToken);
        var categories = (await _categoryRepository.ListAsync(cancellationToken))
            .ToDictionary(c => c.Id);

        var statement = FinancialStatementCalculator.BuildSimplifiedDre(request.Year, request.Month, titles, categories);
        if (statement.IsFailure)
        {
            return Result.Failure<SimplifiedDreReportDto>(statement.Error);
        }

        return Result.Success(FinanceReportMapper.ToDto(statement.Value));
    }
}

public sealed class ExportFinanceStatementsQueryHandler : IRequestHandler<ExportFinanceStatementsQuery, Result<ReportFileDto>>
{
    private readonly IFinancialTitleRepository _titleRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IFinanceStatementPdfRenderer _pdfRenderer;

    public ExportFinanceStatementsQueryHandler(
        IFinancialTitleRepository titleRepository,
        IFinancialCategoryRepository categoryRepository,
        IFinanceStatementPdfRenderer pdfRenderer)
    {
        _titleRepository = titleRepository;
        _categoryRepository = categoryRepository;
        _pdfRenderer = pdfRenderer;
    }

    public async Task<Result<ReportFileDto>> Handle(ExportFinanceStatementsQuery request, CancellationToken cancellationToken)
    {
        var monthValidation = FinancialStatementCalculator.ValidateFullCalendarMonth(request.From, request.To);
        if (monthValidation.IsFailure)
        {
            return Result.Failure<ReportFileDto>(monthValidation.Error);
        }

        var cashFlowResult = await BuildCashFlowAsync(request.From, request.To, cancellationToken);
        if (cashFlowResult.IsFailure)
        {
            return Result.Failure<ReportFileDto>(cashFlowResult.Error);
        }

        var dreResult = await BuildDreAsync(request.From.Year, request.From.Month, cancellationToken);
        if (dreResult.IsFailure)
        {
            return Result.Failure<ReportFileDto>(dreResult.Error);
        }

        var extension = request.Format == FinanceExportFormat.Csv ? "csv" : "pdf";
        var fileName = $"finance-statements-{request.From:yyyy-MM}.{extension}";

        return request.Format switch
        {
            FinanceExportFormat.Csv => Result.Success(new ReportFileDto(
                FinanceStatementCsvExporter.Export(cashFlowResult.Value, dreResult.Value),
                "text/csv",
                fileName)),
            FinanceExportFormat.Pdf => Result.Success(new ReportFileDto(
                _pdfRenderer.Render(cashFlowResult.Value, dreResult.Value),
                "application/pdf",
                fileName)),
            _ => Result.Failure<ReportFileDto>(Finance.Domain.ErrorCodes.Report.InvalidMonth)
        };
    }

    private async Task<Result<CashFlowReportDto>> BuildCashFlowAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var titles = await _titleRepository.ListForStatementsAsync(from, to, cancellationToken);
        var statement = FinancialStatementCalculator.BuildCashFlow(from, to, titles);
        return statement.IsFailure
            ? Result.Failure<CashFlowReportDto>(statement.Error)
            : Result.Success(FinanceReportMapper.ToDto(statement.Value));
    }

    private async Task<Result<SimplifiedDreReportDto>> BuildDreAsync(int year, int month, CancellationToken cancellationToken)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var titles = await _titleRepository.ListForStatementsAsync(from, to, cancellationToken);
        var categories = (await _categoryRepository.ListAsync(cancellationToken)).ToDictionary(c => c.Id);
        var statement = FinancialStatementCalculator.BuildSimplifiedDre(year, month, titles, categories);
        return statement.IsFailure
            ? Result.Failure<SimplifiedDreReportDto>(statement.Error)
            : Result.Success(FinanceReportMapper.ToDto(statement.Value));
    }
}
