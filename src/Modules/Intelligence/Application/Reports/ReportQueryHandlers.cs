using Core.Application.Common.Interfaces;
using Core.Domain;
using Intelligence.Application.Reports.Dtos;
using MediatR;

namespace Intelligence.Application.Reports;

/// <summary>Handles ABC customers report query.</summary>
public sealed class GetAbcCustomersReportQueryHandler : IRequestHandler<GetAbcCustomersReportQuery, Result<AbcCustomerReportDto>>
{
    private readonly IntelligenceReportComposer _composer;

    /// <summary>Creates the handler.</summary>
    public GetAbcCustomersReportQueryHandler(IntelligenceReportComposer composer) => _composer = composer;

    /// <inheritdoc />
    public Task<Result<AbcCustomerReportDto>> Handle(GetAbcCustomersReportQuery request, CancellationToken cancellationToken) =>
        _composer.BuildAbcCustomersAsync(request.From, request.To, cancellationToken);
}

/// <summary>Handles ABC products report query.</summary>
public sealed class GetAbcProductsReportQueryHandler : IRequestHandler<GetAbcProductsReportQuery, Result<AbcProductReportDto>>
{
    private readonly IntelligenceReportComposer _composer;

    /// <summary>Creates the handler.</summary>
    public GetAbcProductsReportQueryHandler(IntelligenceReportComposer composer) => _composer = composer;

    /// <inheritdoc />
    public Task<Result<AbcProductReportDto>> Handle(GetAbcProductsReportQuery request, CancellationToken cancellationToken) =>
        _composer.BuildAbcProductsAsync(request.From, request.To, cancellationToken);
}

/// <summary>Handles productivity report query.</summary>
public sealed class GetProductivityReportQueryHandler : IRequestHandler<GetProductivityReportQuery, Result<ProductivityReportDto>>
{
    private readonly IntelligenceReportComposer _composer;
    private readonly ICurrentUser _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetProductivityReportQueryHandler(IntelligenceReportComposer composer, ICurrentUser currentUser)
    {
        _composer = composer;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public Task<Result<ProductivityReportDto>> Handle(GetProductivityReportQuery request, CancellationToken cancellationToken) =>
        _composer.BuildProductivityAsync(request.From, request.To, _currentUser.TenantId, cancellationToken);
}

/// <summary>Handles ABC customers CSV export.</summary>
public sealed class ExportAbcCustomersReportQueryHandler : IRequestHandler<ExportAbcCustomersReportQuery, Result<IntelligenceReportFileDto>>
{
    private readonly IntelligenceReportComposer _composer;

    /// <summary>Creates the handler.</summary>
    public ExportAbcCustomersReportQueryHandler(IntelligenceReportComposer composer) => _composer = composer;

    /// <inheritdoc />
    public async Task<Result<IntelligenceReportFileDto>> Handle(
        ExportAbcCustomersReportQuery request,
        CancellationToken cancellationToken)
    {
        var report = await _composer.BuildAbcCustomersAsync(request.From, request.To, cancellationToken);
        if (report.IsFailure)
        {
            return Result.Failure<IntelligenceReportFileDto>(report.Error);
        }

        return Result.Success(new IntelligenceReportFileDto(
            IntelligenceReportCsvExporter.ExportAbcCustomers(report.Value),
            $"abc-clientes-{request.From:yyyy-MM-dd}-{request.To:yyyy-MM-dd}.csv",
            "text/csv"));
    }
}

/// <summary>Handles ABC products CSV export.</summary>
public sealed class ExportAbcProductsReportQueryHandler : IRequestHandler<ExportAbcProductsReportQuery, Result<IntelligenceReportFileDto>>
{
    private readonly IntelligenceReportComposer _composer;

    /// <summary>Creates the handler.</summary>
    public ExportAbcProductsReportQueryHandler(IntelligenceReportComposer composer) => _composer = composer;

    /// <inheritdoc />
    public async Task<Result<IntelligenceReportFileDto>> Handle(
        ExportAbcProductsReportQuery request,
        CancellationToken cancellationToken)
    {
        var report = await _composer.BuildAbcProductsAsync(request.From, request.To, cancellationToken);
        if (report.IsFailure)
        {
            return Result.Failure<IntelligenceReportFileDto>(report.Error);
        }

        return Result.Success(new IntelligenceReportFileDto(
            IntelligenceReportCsvExporter.ExportAbcProducts(report.Value),
            $"abc-produtos-{request.From:yyyy-MM-dd}-{request.To:yyyy-MM-dd}.csv",
            "text/csv"));
    }
}

/// <summary>Handles productivity CSV export.</summary>
public sealed class ExportProductivityReportQueryHandler : IRequestHandler<ExportProductivityReportQuery, Result<IntelligenceReportFileDto>>
{
    private readonly IntelligenceReportComposer _composer;
    private readonly ICurrentUser _currentUser;

    /// <summary>Creates the handler.</summary>
    public ExportProductivityReportQueryHandler(IntelligenceReportComposer composer, ICurrentUser currentUser)
    {
        _composer = composer;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Result<IntelligenceReportFileDto>> Handle(
        ExportProductivityReportQuery request,
        CancellationToken cancellationToken)
    {
        var report = await _composer.BuildProductivityAsync(request.From, request.To, _currentUser.TenantId, cancellationToken);
        if (report.IsFailure)
        {
            return Result.Failure<IntelligenceReportFileDto>(report.Error);
        }

        return Result.Success(new IntelligenceReportFileDto(
            IntelligenceReportCsvExporter.ExportProductivity(report.Value),
            $"produtividade-{request.From:yyyy-MM-dd}-{request.To:yyyy-MM-dd}.csv",
            "text/csv"));
    }
}
