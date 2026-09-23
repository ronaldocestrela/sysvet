namespace Core.Application.IntegrationEvents;

/// <summary>Aggregated sales data for ABC and sales-side productivity (10.3).</summary>
public sealed class SalesReportAggregatesSnapshot
{
    /// <summary>Net revenue grouped by tutor id (orders with tutor only).</summary>
    public IReadOnlyList<CustomerRevenueAggregateRow> Customers { get; init; } = Array.Empty<CustomerRevenueAggregateRow>();

    /// <summary>Net revenue and quantity grouped by product id.</summary>
    public IReadOnlyList<ProductRevenueAggregateRow> Products { get; init; } = Array.Empty<ProductRevenueAggregateRow>();

    /// <summary>Sales productivity grouped by performer user id.</summary>
    public IReadOnlyList<SalesProductivityAggregateRow> SalesProductivity { get; init; } = Array.Empty<SalesProductivityAggregateRow>();
}

/// <summary>Customer revenue row from paid POS orders.</summary>
public sealed record CustomerRevenueAggregateRow(Guid TutorId, decimal NetAmount);

/// <summary>Product consumption and revenue from paid POS orders.</summary>
public sealed record ProductRevenueAggregateRow(Guid ProductId, string ProductName, decimal NetAmount, decimal NetQuantity);

/// <summary>Sales-side productivity from order line performers.</summary>
public sealed record SalesProductivityAggregateRow(Guid UserId, decimal NetAmount, decimal NetQuantity);

/// <summary>Completed clinical appointments grouped by veterinarian.</summary>
public sealed class ClinicalProductivitySnapshot
{
    /// <summary>Completed appointment counts by veterinarian user id.</summary>
    public IReadOnlyList<ClinicalProductivityRow> Rows { get; init; } = Array.Empty<ClinicalProductivityRow>();
}

/// <summary>Clinical productivity row.</summary>
public sealed record ClinicalProductivityRow(Guid VeterinarianId, int CompletedCount);

/// <summary>Completed grooming appointments grouped by groomer.</summary>
public sealed class GroomingProductivitySnapshot
{
    /// <summary>Completed grooming counts by groomer user id.</summary>
    public IReadOnlyList<GroomingProductivityRow> Rows { get; init; } = Array.Empty<GroomingProductivityRow>();
}

/// <summary>Grooming productivity row.</summary>
public sealed record GroomingProductivityRow(Guid GroomerId, int CompletedCount);
