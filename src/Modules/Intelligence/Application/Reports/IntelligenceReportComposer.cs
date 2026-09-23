using Core.Application.Entitlements;
using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Entitlements;
using Intelligence.Application.Reports.Dtos;
using Intelligence.Domain.Reports;
using MediatR;

namespace Intelligence.Application.Reports;

/// <summary>Composes intelligence reports from cross-module integration requests (10.3).</summary>
public sealed class IntelligenceReportComposer
{
    private readonly IMediator _mediator;
    private readonly ITenantEntitlementReader _entitlementReader;

    /// <summary>Creates the composer.</summary>
    public IntelligenceReportComposer(IMediator mediator, ITenantEntitlementReader entitlementReader)
    {
        _mediator = mediator;
        _entitlementReader = entitlementReader;
    }

    /// <summary>Builds ABC customer report for the civil range.</summary>
    public async Task<Result<AbcCustomerReportDto>> BuildAbcCustomersAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var rangeValidation = ReportDateRange.Validate(from, to);
        if (rangeValidation.IsFailure)
        {
            return Result.Failure<AbcCustomerReportDto>(rangeValidation.Error);
        }

        var (startUtc, endUtc) = ReportDateRange.ToUtcBounds(from, to);
        var sales = await _mediator.Send(new GetSalesReportAggregatesRequest(startUtc, endUtc), cancellationToken);
        if (sales.IsFailure)
        {
            return Result.Failure<AbcCustomerReportDto>(sales.Error);
        }

        var abcInputs = sales.Value.Customers
            .Select(c => new AbcCurveInputRow(c.TutorId, c.NetAmount, c.TutorId.ToString("D")))
            .ToList();
        var classified = AbcCurveClassifier.Classify(abcInputs);

        var names = await _mediator.Send(
            new GetTutorDisplayNamesRequest(classified.Select(r => r.Id).ToList()),
            cancellationToken);
        if (names.IsFailure)
        {
            return Result.Failure<AbcCustomerReportDto>(names.Error);
        }

        var rows = classified.Select(r => new AbcCustomerRowDto(
            r.Id,
            names.Value.TryGetValue(r.Id, out var name) ? name : r.Id.ToString("D"),
            r.Value,
            r.SharePercent,
            r.CumulativeSharePercent,
            r.Class,
            r.Rank)).ToList();

        return Result.Success(new AbcCustomerReportDto(from, to, rows));
    }

    /// <summary>Builds ABC product report for the civil range.</summary>
    public async Task<Result<AbcProductReportDto>> BuildAbcProductsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var rangeValidation = ReportDateRange.Validate(from, to);
        if (rangeValidation.IsFailure)
        {
            return Result.Failure<AbcProductReportDto>(rangeValidation.Error);
        }

        var (startUtc, endUtc) = ReportDateRange.ToUtcBounds(from, to);
        var sales = await _mediator.Send(new GetSalesReportAggregatesRequest(startUtc, endUtc), cancellationToken);
        if (sales.IsFailure)
        {
            return Result.Failure<AbcProductReportDto>(sales.Error);
        }

        var productMap = sales.Value.Products.ToDictionary(p => p.ProductId);
        var abcInputs = sales.Value.Products
            .Select(p => new AbcCurveInputRow(p.ProductId, p.NetAmount, p.ProductId.ToString("D")))
            .ToList();
        var classified = AbcCurveClassifier.Classify(abcInputs);

        var rows = classified.Select(r =>
        {
            var source = productMap[r.Id];
            return new AbcProductRowDto(
                r.Id,
                source.ProductName,
                r.Value,
                source.NetQuantity,
                r.SharePercent,
                r.CumulativeSharePercent,
                r.Class,
                r.Rank);
        }).ToList();

        return Result.Success(new AbcProductReportDto(from, to, rows));
    }

    /// <summary>Builds productivity report respecting module entitlements.</summary>
    public async Task<Result<ProductivityReportDto>> BuildProductivityAsync(
        DateOnly from,
        DateOnly to,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rangeValidation = ReportDateRange.Validate(from, to);
        if (rangeValidation.IsFailure)
        {
            return Result.Failure<ProductivityReportDto>(rangeValidation.Error);
        }

        var enabled = tenantId == Guid.Empty
            ? new HashSet<CommercialModule>(Enum.GetValues<CommercialModule>())
            : await _entitlementReader.GetEnabledModulesAsync(tenantId, cancellationToken);

        var (startUtc, endUtc) = ReportDateRange.ToUtcBounds(from, to);
        var contributions = new List<ProductivityContribution>();

        if (enabled.Contains(CommercialModule.Sales))
        {
            var sales = await _mediator.Send(new GetSalesReportAggregatesRequest(startUtc, endUtc), cancellationToken);
            if (sales.IsFailure)
            {
                return Result.Failure<ProductivityReportDto>(sales.Error);
            }

            contributions.AddRange(sales.Value.SalesProductivity.Select(row =>
                new ProductivityContribution(row.UserId, row.NetAmount, row.NetQuantity, 0, 0)));
        }

        if (enabled.Contains(CommercialModule.Veterinary))
        {
            var clinical = await _mediator.Send(new GetClinicalProductivityRequest(startUtc, endUtc), cancellationToken);
            if (clinical.IsFailure)
            {
                return Result.Failure<ProductivityReportDto>(clinical.Error);
            }

            contributions.AddRange(clinical.Value.Rows.Select(row =>
                new ProductivityContribution(row.VeterinarianId, 0m, 0m, row.CompletedCount, 0)));
        }

        if (enabled.Contains(CommercialModule.Petshop))
        {
            var grooming = await _mediator.Send(new GetGroomingProductivityRequest(startUtc, endUtc), cancellationToken);
            if (grooming.IsFailure)
            {
                return Result.Failure<ProductivityReportDto>(grooming.Error);
            }

            contributions.AddRange(grooming.Value.Rows.Select(row =>
                new ProductivityContribution(row.GroomerId, 0m, 0m, 0, row.CompletedCount)));
        }

        var merged = ProductivityMerger.Merge(contributions);
        var names = await _mediator.Send(
            new GetStaffDisplayNamesRequest(merged.Select(r => r.UserId).ToList()),
            cancellationToken);
        if (names.IsFailure)
        {
            return Result.Failure<ProductivityReportDto>(names.Error);
        }

        var rows = merged.Select(r => new ProductivityRowDto(
            r.UserId,
            names.Value.TryGetValue(r.UserId, out var name) ? name : r.UserId.ToString("D"),
            r.SalesNetAmount,
            r.SalesQuantity,
            r.ClinicalCompleted,
            r.GroomingCompleted)).ToList();

        return Result.Success(new ProductivityReportDto(from, to, rows));
    }
}
