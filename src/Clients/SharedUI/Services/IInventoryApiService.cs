using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public interface IInventoryApiService
{
    Task<List<ProductDto>> GetProductsAsync();
    Task<bool> RegisterProductAsync(string name, string description, string barcode, string unit, decimal reorderLevel);
    
    Task<List<StockMovementDto>> GetRecentMovementsAsync(Guid? productId = null);
    Task<bool> RegisterStockMovementAsync(Guid productId, string type, decimal quantity, string batchNumber, DateTimeOffset? expirationDate, string reason);
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal ReorderLevel { get; set; }
    public decimal CurrentStock { get; set; }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTimeOffset? ExpirationDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
}
