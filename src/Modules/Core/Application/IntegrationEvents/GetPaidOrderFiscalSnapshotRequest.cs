using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Loads paid order lines for fiscal emission (Sales module).</summary>
public sealed class GetPaidOrderFiscalSnapshotRequest : IRequest<Result<PaidOrderFiscalSnapshot>>
{
    public Guid OrderId { get; }

    public GetPaidOrderFiscalSnapshotRequest(Guid orderId) => OrderId = orderId;
}

/// <summary>Paid order data needed to compose NF-e/NFS-e.</summary>
public sealed class PaidOrderFiscalSnapshot
{
    public Guid OrderId { get; init; }
    public Guid? TutorId { get; init; }
    public bool IsPaid { get; init; }
    public IReadOnlyList<PaidOrderFiscalLine> Lines { get; init; } = Array.Empty<PaidOrderFiscalLine>();
}

/// <summary>Single order line for fiscal split.</summary>
public sealed class PaidOrderFiscalLine
{
    public string Kind { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
