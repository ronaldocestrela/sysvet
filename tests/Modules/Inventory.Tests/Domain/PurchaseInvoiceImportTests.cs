using FluentAssertions;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.ValueObjects;

namespace Inventory.Tests.Domain;

public class PurchaseInvoiceImportTests
{
    private static readonly AccessKey Key = AccessKey.Create("35250900000000000000550010000000011000000001").Value;

    [Fact]
    public void Confirm_WhenAllLinesMapped_Succeeds()
    {
        var importId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var line = PurchaseInvoiceImportLine.Create(importId, 1, "P1", "7891234567890", "Item", "23091000", "UN", 10m, 5m, 50m, null, null);
        line.AssignProduct(productId);
        line.MarkStockApplied(null, Guid.NewGuid());

        var import = PurchaseInvoiceImport.CreateDraft(
            Key,
            "Legal",
            "Trade",
            "11222333000181",
            "1",
            "1",
            DateTimeOffset.UtcNow,
            50m,
            "blob/key",
            [line],
            Guid.NewGuid(),
            importId).Value;

        var result = import.Confirm();
        result.IsSuccess.Should().BeTrue();
        import.Status.Should().Be(PurchaseImportStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WithoutProduct_Fails()
    {
        var importId = Guid.NewGuid();
        var line = PurchaseInvoiceImportLine.Create(importId, 1, "P1", null, "Item", "23091000", "UN", 10m, 5m, 50m, null, null);

        var import = PurchaseInvoiceImport.CreateDraft(
            Key,
            "Legal",
            "Trade",
            "11222333000181",
            "1",
            "1",
            DateTimeOffset.UtcNow,
            50m,
            "blob/key",
            [line],
            Guid.NewGuid(),
            importId).Value;

        import.Confirm().IsFailure.Should().BeTrue();
    }
}
