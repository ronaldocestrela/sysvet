using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.Entities;

public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Barcode { get; private set; }
    public string UnitOfMeasure { get; private set; }
    public decimal ReorderLevel { get; private set; }

    private Product(string name, string description, string barcode, string unitOfMeasure, decimal reorderLevel)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Barcode = barcode;
        UnitOfMeasure = unitOfMeasure;
        ReorderLevel = reorderLevel;
    }

    public static Result<Product> Create(string name, string description, string barcode, string unitOfMeasure, decimal reorderLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(ErrorCodes.Product.InvalidName);

        if (string.IsNullOrWhiteSpace(barcode))
            return Result.Failure<Product>(ErrorCodes.Product.InvalidBarcode);

        if (reorderLevel < 0)
            return Result.Failure<Product>(ErrorCodes.Product.InvalidReorderLevel);

        return Result.Success(new Product(name, description, barcode, unitOfMeasure, reorderLevel));
    }
}
