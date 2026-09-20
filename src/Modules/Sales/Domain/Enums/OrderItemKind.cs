namespace Sales.Domain.Enums;

/// <summary>Line type: physical product (stock), service, kit (multi-product stock), or prepaid package.</summary>
public enum OrderItemKind
{
    Product = 0,
    Service = 1,
    Kit = 2,
    Package = 3
}
