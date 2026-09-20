using Core.Domain;
using Sales.Domain.Enums;

namespace Sales.Domain.Entities;

/// <summary>Tenant-scoped prepaid service offer (credits uses on package sale).</summary>
public sealed class ServicePackage : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public ServiceCode ServiceCode { get; private set; }
    public int UsesPerUnit { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ServicePackage() { }

    private ServicePackage(Guid id, string name, ServiceCode serviceCode, int usesPerUnit, bool isActive)
        : base(id)
    {
        Name = name;
        ServiceCode = serviceCode;
        UsesPerUnit = usesPerUnit;
        IsActive = isActive;
    }

    /// <summary>Creates a service package offer.</summary>
    public static Result<ServicePackage> Create(string name, ServiceCode serviceCode, int usesPerUnit)
        => Create(Guid.NewGuid(), name, serviceCode, usesPerUnit, true);

    /// <summary>Creates a package with a known id (sync).</summary>
    public static Result<ServicePackage> Create(
        Guid id,
        string name,
        ServiceCode serviceCode,
        int usesPerUnit,
        bool isActive)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<ServicePackage>(ErrorCodes.Package.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ServicePackage>(ErrorCodes.Package.NameRequired);
        }

        if (usesPerUnit <= 0)
        {
            return Result.Failure<ServicePackage>(ErrorCodes.Package.InvalidUsesPerUnit);
        }

        return Result.Success(new ServicePackage(id, name.Trim(), serviceCode, usesPerUnit, isActive));
    }

    /// <summary>Updates metadata.</summary>
    public Result Update(string name, ServiceCode serviceCode, int usesPerUnit, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.Package.NameRequired);
        }

        if (usesPerUnit <= 0)
        {
            return Result.Failure(ErrorCodes.Package.InvalidUsesPerUnit);
        }

        Name = name.Trim();
        ServiceCode = serviceCode;
        UsesPerUnit = usesPerUnit;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
