using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Entities;

/// <summary>
/// Formal prescription document linked to an appointment (distinct from free-text conduct on the medical record).
/// </summary>
public sealed class IssuedPrescription : AggregateRoot
{
    private readonly List<PrescriptionItem> _items = new();

    /// <summary>Consultation appointment.</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Prescribing veterinarian.</summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>Optional source template.</summary>
    public Guid? TemplateId { get; private set; }

    /// <summary>Draft allows edits; issued is immutable.</summary>
    public IssuedPrescriptionStatus Status { get; private set; }

    /// <summary>Prescription lines.</summary>
    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    private IssuedPrescription() { }

    private IssuedPrescription(Guid id, Guid appointmentId, Guid petId, Guid veterinarianId, Guid? templateId)
        : base(id)
    {
        AppointmentId = appointmentId;
        PetId = petId;
        VeterinarianId = veterinarianId;
        TemplateId = templateId;
        Status = IssuedPrescriptionStatus.Draft;
    }

    /// <summary>Creates a new draft prescription for a visit.</summary>
    public static Result<IssuedPrescription> Create(Guid id, Guid appointmentId, Guid petId, Guid veterinarianId, Guid? templateId = null)
    {
        if (appointmentId == Guid.Empty || petId == Guid.Empty || veterinarianId == Guid.Empty)
        {
            return Result.Failure<IssuedPrescription>(ErrorCodes.IssuedPrescription.InvalidIdentifiers);
        }

        var prescription = new IssuedPrescription(id, appointmentId, petId, veterinarianId, templateId);
        prescription.Touch();
        return Result.Success(prescription);
    }

    /// <summary>Rehydrates from sync pull.</summary>
    public static IssuedPrescription RestoreFromSync(
        Guid id,
        Guid appointmentId,
        Guid petId,
        Guid veterinarianId,
        Guid? templateId,
        IssuedPrescriptionStatus status,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        var prescription = new IssuedPrescription(id, appointmentId, petId, veterinarianId, templateId)
        {
            Status = status,
            UpdatedAt = updatedAt
        };

        foreach (var item in items)
        {
            var created = PrescriptionItem.Create(
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
                prescription._items.Add(created.Value);
            }
        }

        return prescription;
    }

    /// <summary>Applies remote sync snapshot; never downgrades issued to draft.</summary>
    public void ApplySyncSnapshot(
        Guid appointmentId,
        Guid petId,
        Guid veterinarianId,
        Guid? templateId,
        IssuedPrescriptionStatus status,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        if (Status == IssuedPrescriptionStatus.Issued && status == IssuedPrescriptionStatus.Draft)
        {
            status = IssuedPrescriptionStatus.Issued;
        }

        AppointmentId = appointmentId;
        PetId = petId;
        VeterinarianId = veterinarianId;
        TemplateId = templateId;
        Status = status;
        UpdatedAt = updatedAt;
        _items.Clear();

        foreach (var item in items)
        {
            var created = PrescriptionItem.Create(
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

    /// <summary>Replaces draft line items.</summary>
    public Result ReplaceDraftItems(IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.IssuedPrescription.AlreadyIssued);
        }

        _items.Clear();
        var order = 0;
        foreach (var item in items)
        {
            var created = PrescriptionItem.Create(
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

    /// <summary>Copies items from a template into this draft.</summary>
    public Result CopyItemsFromTemplate(PrescriptionTemplate template)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.IssuedPrescription.AlreadyIssued);
        }

        _items.Clear();
        var order = 0;
        foreach (var templateItem in template.Items.OrderBy(i => i.SortOrder))
        {
            var created = PrescriptionItem.FromTemplateItem(Guid.NewGuid(), Id, templateItem, order++);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _items.Add(created.Value);
        }

        TemplateId = template.Id;
        Touch();
        return Result.Success();
    }

    /// <summary>Locks the prescription for printing and legal record.</summary>
    public Result Issue()
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.IssuedPrescription.AlreadyIssued);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(ErrorCodes.IssuedPrescription.EmptyItems);
        }

        Status = IssuedPrescriptionStatus.Issued;
        Touch();
        return Result.Success();
    }

    private bool EnsureDraft() => Status != IssuedPrescriptionStatus.Issued;

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
