using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>
/// Tenant-scoped reusable prescription model (items copied into issued prescriptions).
/// </summary>
public sealed class PrescriptionTemplate : AggregateRoot
{
    private readonly List<PrescriptionTemplateItem> _items = new();

    /// <summary>Display name for staff selection.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional species filter (empty = any).</summary>
    public string Species { get; private set; } = string.Empty;

    /// <summary>Whether the template appears in pick lists.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Template line items.</summary>
    public IReadOnlyCollection<PrescriptionTemplateItem> Items => _items.AsReadOnly();

    private PrescriptionTemplate() { }

    private PrescriptionTemplate(Guid id, string name, string species)
        : base(id)
    {
        Name = name;
        Species = species;
        IsActive = true;
    }

    /// <summary>Creates a new active template.</summary>
    public static Result<PrescriptionTemplate> Create(Guid id, string name, string? species = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<PrescriptionTemplate>(ErrorCodes.PrescriptionTemplate.InvalidName);
        }

        if (name.Length > 200)
        {
            return Result.Failure<PrescriptionTemplate>(ErrorCodes.PrescriptionTemplate.InvalidName);
        }

        var template = new PrescriptionTemplate(id, name.Trim(), species?.Trim() ?? string.Empty);
        template.Touch();
        return Result.Success(template);
    }

    /// <summary>Rehydrates from sync without validation.</summary>
    public static PrescriptionTemplate RestoreFromSync(
        Guid id,
        string name,
        string species,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        var template = new PrescriptionTemplate(id, name, species)
        {
            IsActive = isActive,
            UpdatedAt = updatedAt
        };

        foreach (var item in items)
        {
            var created = PrescriptionTemplateItem.Create(
                item.ItemId,
                id,
                item.MedicationName,
                item.Concentration,
                item.Dose,
                item.Route,
                item.Frequency,
                item.Duration,
                item.Instructions,
                item.SortOrder);
            if (created.IsSuccess)
            {
                template._items.Add(created.Value);
            }
        }

        return template;
    }

    /// <summary>Applies remote sync snapshot (LWW).</summary>
    public void ApplySyncSnapshot(
        string name,
        string species,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        Name = name;
        Species = species;
        IsActive = isActive;
        UpdatedAt = updatedAt;
        _items.Clear();

        foreach (var item in items)
        {
            var created = PrescriptionTemplateItem.Create(
                item.ItemId,
                Id,
                item.MedicationName,
                item.Concentration,
                item.Dose,
                item.Route,
                item.Frequency,
                item.Duration,
                item.Instructions,
                item.SortOrder);
            if (created.IsSuccess)
            {
                _items.Add(created.Value);
            }
        }
    }

    /// <summary>Renames the template and optional species filter.</summary>
    public Result UpdateDetails(string name, string? species)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure(ErrorCodes.PrescriptionTemplate.InvalidName);
        }

        Name = name.Trim();
        Species = species?.Trim() ?? string.Empty;
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces all line items (full snapshot).</summary>
    public Result ReplaceItems(IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        _items.Clear();
        var order = 0;
        foreach (var item in items)
        {
            var created = PrescriptionTemplateItem.Create(
                item.ItemId == Guid.Empty ? Guid.NewGuid() : item.ItemId,
                Id,
                item.MedicationName,
                item.Concentration,
                item.Dose,
                item.Route,
                item.Frequency,
                item.Duration,
                item.Instructions,
                item.SortOrder == 0 ? order++ : item.SortOrder);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _items.Add(created.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Marks the template inactive (soft catalog removal).</summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(ErrorCodes.PrescriptionTemplate.AlreadyInactive);
        }

        IsActive = false;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
