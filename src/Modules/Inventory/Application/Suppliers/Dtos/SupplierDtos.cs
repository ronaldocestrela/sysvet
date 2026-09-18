namespace Inventory.Application.Suppliers.Dtos;

/// <summary>Supplier list/detail DTO.</summary>
public sealed record SupplierDto(
    Guid Id,
    string LegalName,
    string TradeName,
    string Document,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive);
