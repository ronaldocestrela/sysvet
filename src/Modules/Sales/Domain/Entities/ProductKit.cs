using Core.Domain;

namespace Sales.Domain.Entities;

/// <summary>Tenant-scoped sellable kit that explodes into inventory products at pay time.</summary>
public sealed class ProductKit : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<KitComponent> _components = new();
    public IReadOnlyCollection<KitComponent> Components => _components.AsReadOnly();

    private ProductKit() { }

    private ProductKit(Guid id, string name, bool isActive)
        : base(id)
    {
        Name = name;
        IsActive = isActive;
    }

    /// <summary>Creates a new kit definition.</summary>
    public static Result<ProductKit> Create(string name, IEnumerable<(Guid ProductId, decimal QuantityPerKit)> components)
        => Create(Guid.NewGuid(), name, true, components);

    /// <summary>Creates or replaces a kit with a known id (sync).</summary>
    public static Result<ProductKit> Create(
        Guid id,
        string name,
        bool isActive,
        IEnumerable<(Guid ProductId, decimal QuantityPerKit)> components)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<ProductKit>(ErrorCodes.Kit.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ProductKit>(ErrorCodes.Kit.NameRequired);
        }

        var componentList = components?.ToList() ?? new List<(Guid, decimal)>();
        if (componentList.Count == 0)
        {
            return Result.Failure<ProductKit>(ErrorCodes.Kit.EmptyComponents);
        }

        var kit = new ProductKit(id, name.Trim(), isActive);
        foreach (var (productId, qty) in componentList)
        {
            var comp = KitComponent.Create(id, productId, qty);
            if (comp.IsFailure)
            {
                return Result.Failure<ProductKit>(comp.Error);
            }

            kit._components.Add(comp.Value);
        }

        return Result.Success(kit);
    }

    /// <summary>Updates display name and active flag.</summary>
    public Result Update(string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.Kit.NameRequired);
        }

        Name = name.Trim();
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Replaces all components (kit must stay non-empty).</summary>
    public Result ReplaceComponents(IEnumerable<(Guid ProductId, decimal QuantityPerKit)> components)
    {
        var componentList = components?.ToList() ?? new List<(Guid, decimal)>();
        if (componentList.Count == 0)
        {
            return Result.Failure(ErrorCodes.Kit.EmptyComponents);
        }

        _components.Clear();
        foreach (var (productId, qty) in componentList)
        {
            var comp = KitComponent.Create(Id, productId, qty);
            if (comp.IsFailure)
            {
                return Result.Failure(comp.Error);
            }

            _components.Add(comp.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Expands kit quantity into stock lines (product id × total qty).</summary>
    public IReadOnlyList<(Guid ProductId, decimal Quantity)> ExplodeStockLines(decimal kitQuantity)
    {
        if (kitQuantity <= 0)
        {
            return Array.Empty<(Guid, decimal)>();
        }

        return _components
            .Select(c => (c.ProductId, c.QuantityPerKit * kitQuantity))
            .ToList();
    }
}
