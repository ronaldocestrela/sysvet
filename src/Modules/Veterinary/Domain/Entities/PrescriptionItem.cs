using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Line item on an issued prescription (snapshot at issue time).</summary>
public sealed class PrescriptionItem : Entity
{
    /// <summary>Parent issued prescription identifier.</summary>
    public Guid IssuedPrescriptionId { get; private set; }

    /// <summary>Medication or product name.</summary>
    public string MedicationName { get; private set; } = string.Empty;

    /// <summary>Concentration or strength.</summary>
    public string Concentration { get; private set; } = string.Empty;

    /// <summary>Dose instructions.</summary>
    public string Dose { get; private set; } = string.Empty;

    /// <summary>Route of administration.</summary>
    public string Route { get; private set; } = string.Empty;

    /// <summary>Frequency.</summary>
    public string Frequency { get; private set; } = string.Empty;

    /// <summary>Duration.</summary>
    public string Duration { get; private set; } = string.Empty;

    /// <summary>Additional instructions.</summary>
    public string Instructions { get; private set; } = string.Empty;

    /// <summary>Display order.</summary>
    public int SortOrder { get; private set; }

    private PrescriptionItem() { }

    private PrescriptionItem(
        Guid id,
        Guid issuedPrescriptionId,
        string medicationName,
        string concentration,
        string dose,
        string route,
        string frequency,
        string duration,
        string instructions,
        int sortOrder)
        : base(id)
    {
        IssuedPrescriptionId = issuedPrescriptionId;
        MedicationName = medicationName;
        Concentration = concentration;
        Dose = dose;
        Route = route;
        Frequency = frequency;
        Duration = duration;
        Instructions = instructions;
        SortOrder = sortOrder;
    }

    /// <summary>Creates a validated prescription line.</summary>
    public static Result<PrescriptionItem> Create(
        Guid id,
        Guid issuedPrescriptionId,
        string medicationName,
        string concentration,
        string dose,
        string route,
        string frequency,
        string duration,
        string instructions,
        int sortOrder)
    {
        if (issuedPrescriptionId == Guid.Empty)
        {
            return Result.Failure<PrescriptionItem>(ErrorCodes.IssuedPrescription.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            return Result.Failure<PrescriptionItem>(ErrorCodes.IssuedPrescription.EmptyItems);
        }

        return Result.Success(new PrescriptionItem(
            id,
            issuedPrescriptionId,
            medicationName.Trim(),
            concentration.Trim(),
            dose.Trim(),
            route.Trim(),
            frequency.Trim(),
            duration.Trim(),
            instructions.Trim(),
            sortOrder));
    }

    /// <summary>Copies a template item into a draft prescription line.</summary>
    public static Result<PrescriptionItem> FromTemplateItem(Guid id, Guid issuedPrescriptionId, PrescriptionTemplateItem templateItem, int sortOrder) =>
        Create(
            id,
            issuedPrescriptionId,
            templateItem.MedicationName,
            templateItem.Concentration,
            templateItem.Dose,
            templateItem.Route,
            templateItem.Frequency,
            templateItem.Duration,
            templateItem.Instructions,
            sortOrder);
}
