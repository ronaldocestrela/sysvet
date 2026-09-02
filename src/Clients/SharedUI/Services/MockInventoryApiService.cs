using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public class MockInventoryApiService : IInventoryApiService
{
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        await Task.Delay(500); // Simulate network
        return new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "Vacina V10", Description = "Vacina Polivalente", Barcode = "7891234567890", UnitOfMeasure = "Dose", ReorderLevel = 10, CurrentStock = 25 },
            new ProductDto { Id = Guid.NewGuid(), Name = "Antipulgas Bravecto 10-20kg", Description = "Comprimido mastigável", Barcode = "7890987654321", UnitOfMeasure = "Caixa", ReorderLevel = 5, CurrentStock = 3 },
            new ProductDto { Id = Guid.NewGuid(), Name = "Ração Golden Duo Adultos", Description = "15kg Sabor Carne e Frango", Barcode = "7895555555555", UnitOfMeasure = "Saco", ReorderLevel = 10, CurrentStock = 12 }
        };
    }

    public async Task<bool> RegisterProductAsync(string name, string description, string barcode, string unit, decimal reorderLevel)
    {
        await Task.Delay(500);
        return true;
    }

    public async Task<List<StockMovementDto>> GetRecentMovementsAsync(Guid? productId = null)
    {
        await Task.Delay(500);
        return new List<StockMovementDto>
        {
            new StockMovementDto { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Vacina V10", Type = "In", Quantity = 50, BatchNumber = "L1023", ExpirationDate = DateTimeOffset.Now.AddMonths(12), Date = DateTimeOffset.Now.AddDays(-2), Reason = "Compra Fornecedor" },
            new StockMovementDto { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Antipulgas Bravecto 10-20kg", Type = "Out", Quantity = 1, BatchNumber = "", ExpirationDate = null, Date = DateTimeOffset.Now.AddHours(-5), Reason = "Venda PDV" }
        };
    }

    public async Task<bool> RegisterStockMovementAsync(Guid productId, string type, decimal quantity, string batchNumber, DateTimeOffset? expirationDate, string reason)
    {
        await Task.Delay(500);
        return true;
    }
}
