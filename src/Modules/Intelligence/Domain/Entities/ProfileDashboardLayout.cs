using System.Text.Json;
using Core.Domain;
using Intelligence.Domain.Dashboard;

namespace Intelligence.Domain.Entities;

/// <summary>
/// Tenant-scoped dashboard widget layout bound to an <see cref="Core.Domain.Entities.AccessProfile"/>.
/// </summary>
public sealed class ProfileDashboardLayout : AggregateRoot
{
    /// <summary>Access profile this layout applies to.</summary>
    public Guid AccessProfileId { get; private set; }

    private readonly List<DashboardWidgetSlot> _slots = new();
    private string _slotsJson = "[]";

    /// <summary>Ordered widget visibility for the profile.</summary>
    public IReadOnlyList<DashboardWidgetSlot> Slots => _slots.AsReadOnly();

    /// <summary>JSON persistence column mapped by EF Core.</summary>
    public string SlotsStorage
    {
        get => _slotsJson;
        private set
        {
            _slotsJson = string.IsNullOrWhiteSpace(value) ? "[]" : value;
            var deserialized = JsonSerializer.Deserialize<List<DashboardWidgetSlot>>(_slotsJson) ?? [];
            _slots.Clear();
            _slots.AddRange(deserialized);
        }
    }

    private ProfileDashboardLayout(Guid id, Guid accessProfileId) : base(id)
    {
        AccessProfileId = accessProfileId;
    }

    /// <summary>Creates a validated layout for a profile.</summary>
    public static Result<ProfileDashboardLayout> Create(Guid accessProfileId, IEnumerable<DashboardWidgetSlot> slots)
    {
        var slotList = slots.ToList();
        var validation = ValidateSlots(slotList);
        if (validation.IsFailure)
        {
            return Result.Failure<ProfileDashboardLayout>(validation.Error);
        }

        var layout = new ProfileDashboardLayout(Guid.NewGuid(), accessProfileId);
        layout.ApplySlots(slotList);
        return Result.Success(layout);
    }

    /// <summary>Replaces widget slots after validation.</summary>
    public Result UpdateSlots(IEnumerable<DashboardWidgetSlot> slots)
    {
        var slotList = slots.ToList();
        var validation = ValidateSlots(slotList);
        if (validation.IsFailure)
        {
            return validation;
        }

        ApplySlots(slotList);
        return Result.Success();
    }

    private static Result ValidateSlots(IReadOnlyList<DashboardWidgetSlot> slots)
    {
        if (slots.Count == 0)
        {
            return Result.Failure(ErrorCodes.DashboardLayout.EmptyLayout);
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var slot in slots)
        {
            if (!WidgetCatalog.IsValid(slot.WidgetKey))
            {
                return Result.Failure(ErrorCodes.DashboardLayout.UnknownWidget);
            }

            if (!seen.Add(slot.WidgetKey))
            {
                return Result.Failure(ErrorCodes.DashboardLayout.DuplicateWidget);
            }
        }

        return Result.Success();
    }

    private void ApplySlots(IReadOnlyList<DashboardWidgetSlot> slots)
    {
        _slots.Clear();
        _slots.AddRange(slots.OrderBy(s => s.SortOrder));
        _slotsJson = JsonSerializer.Serialize(_slots);
    }
}
