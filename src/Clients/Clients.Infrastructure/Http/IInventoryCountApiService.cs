using Core.Domain;
using Inventory.Domain.Enums;

namespace Clients.Infrastructure.Http;

/// <summary>
/// Online API for physical inventory count sessions (requires connectivity).
/// </summary>
public interface IInventoryCountApiService
{
    Task<Result<Guid>> StartAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryCountListItemClientDto>>> ListAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<Result<InventoryCountDetailClientDto>> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> AddLineAsync(Guid sessionId, AddInventoryCountLineClientRequest request, CancellationToken cancellationToken = default);
    Task<Result> SubmitAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result> ApproveAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<InventoryProductByBarcodeClientDto>> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}

public sealed class InventoryCountListItemClientDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public InventoryCountStatus Status { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int LineCount { get; init; }
}

public sealed class InventoryCountDetailClientDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public InventoryCountStatus Status { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<InventoryCountLineClientDto> Lines { get; init; } = Array.Empty<InventoryCountLineClientDto>();
}

public sealed class InventoryCountLineClientDto
{
    public Guid LineId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public Guid? ProductLotId { get; init; }
    public string? LotNumber { get; init; }
    public decimal CountedQuantity { get; init; }
    public decimal? ExpectedQuantity { get; init; }
    public decimal? Variance { get; init; }
}

public sealed class AddInventoryCountLineClientRequest
{
    public string? Barcode { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? ProductLotId { get; init; }
    public decimal QuantityToAdd { get; init; } = 1m;
}

public sealed class InventoryProductByBarcodeClientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public bool RequiresLot { get; init; }
    public IReadOnlyList<InventoryProductLotClientDto> Lots { get; init; } = Array.Empty<InventoryProductLotClientDto>();
}

public sealed class InventoryProductLotClientDto
{
    public Guid Id { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public bool IsActive { get; init; }
}
