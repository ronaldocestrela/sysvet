using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Tenant-scoped ward catalog with beds for the execution map.</summary>
public sealed class WardUnit : AggregateRoot
{
    private readonly List<Bed> _beds = new();

    /// <summary>Display name (e.g. ICU, Ward A).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Whether the unit appears in configuration lists.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Beds belonging to this unit.</summary>
    public IReadOnlyCollection<Bed> Beds => _beds.AsReadOnly();

    private WardUnit() { }

    private WardUnit(Guid id, string name)
        : base(id)
    {
        Name = name;
        IsActive = true;
    }

    /// <summary>Creates a new active ward unit.</summary>
    public static Result<WardUnit> Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure<WardUnit>(ErrorCodes.WardUnit.InvalidName);
        }

        var unit = new WardUnit(id, name.Trim());
        unit.Touch();
        return Result.Success(unit);
    }

    /// <summary>Rehydrates from sync pull.</summary>
    public static WardUnit RestoreFromSync(
        Guid id,
        string name,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid BedId, string Code, int SortOrder, bool IsActive)> beds)
    {
        var unit = new WardUnit(id, name) { IsActive = isActive, UpdatedAt = updatedAt };
        foreach (var bed in beds)
        {
            unit._beds.Add(Bed.RestoreFromSync(bed.BedId, id, bed.Code, bed.SortOrder, bed.IsActive, updatedAt));
        }

        return unit;
    }

    /// <summary>Applies remote sync snapshot (LWW).</summary>
    public void ApplySyncSnapshot(
        string name,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid BedId, string Code, int SortOrder, bool IsActive)> beds)
    {
        Name = name;
        IsActive = isActive;
        UpdatedAt = updatedAt;
        _beds.Clear();
        foreach (var bed in beds)
        {
            _beds.Add(Bed.RestoreFromSync(bed.BedId, Id, bed.Code, bed.SortOrder, bed.IsActive, updatedAt));
        }
    }

    /// <summary>Renames the ward unit.</summary>
    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure(ErrorCodes.WardUnit.InvalidName);
        }

        Name = name.Trim();
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces all bed lines (same pattern as vaccine protocol doses).</summary>
    public Result ReplaceBeds(IEnumerable<(Guid BedId, string Code, int SortOrder, bool IsActive)> beds)
    {
        _beds.Clear();
        foreach (var line in beds)
        {
            var created = Bed.Create(
                line.BedId == Guid.Empty ? Guid.NewGuid() : line.BedId,
                Id,
                line.Code,
                line.SortOrder);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            var bed = created.Value;
            if (!line.IsActive)
            {
                bed.Update(line.Code, line.SortOrder, false);
            }

            _beds.Add(bed);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Soft-deactivates the unit and all beds.</summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(ErrorCodes.WardUnit.AlreadyInactive);
        }

        IsActive = false;
        foreach (var bed in _beds.ToList())
        {
            bed.Update(bed.Code, bed.SortOrder, false);
        }

        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
