using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Line item within a reusable prescription template.</summary>
public sealed class PrescriptionTemplateItem : Entity
{
    /// <summary>Parent template identifier.</summary>
    public Guid PrescriptionTemplateId { get; private set; }

    /// <summary>Medication or product name.</summary>
    public string MedicationName { get; private set; } = string.Empty;

    /// <summary>Concentration or strength (e.g. 50mg/ml).</summary>
    public string Concentration { get; private set; } = string.Empty;

    /// <summary>Dose instructions.</summary>
    public string Dose { get; private set; } = string.Empty;

    /// <summary>Route of administration.</summary>
    public string Route { get; private set; } = string.Empty;

    /// <summary>Frequency (e.g. every 12h).</summary>
    public string Frequency { get; private set; } = string.Empty;

    /// <summary>Duration of treatment.</summary>
    public string Duration { get; private set; } = string.Empty;

    /// <summary>Additional instructions for the tutor.</summary>
    public string Instructions { get; private set; } = string.Empty;

    /// <summary>Display order within the template.</summary>
    public int SortOrder { get; private set; }

    private PrescriptionTemplateItem() { }

    private PrescriptionTemplateItem(
        Guid id,
        Guid templateId,
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
        PrescriptionTemplateId = templateId;
        MedicationName = medicationName;
        Concentration = concentration;
        Dose = dose;
        Route = route;
        Frequency = frequency;
        Duration = duration;
        Instructions = instructions;
        SortOrder = sortOrder;
    }

    /// <summary>Creates a validated template line item.</summary>
    public static Result<PrescriptionTemplateItem> Create(
        Guid id,
        Guid templateId,
        string medicationName,
        string concentration,
        string dose,
        string route,
        string frequency,
        string duration,
        string instructions,
        int sortOrder)
    {
        if (templateId == Guid.Empty)
        {
            return Result.Failure<PrescriptionTemplateItem>(ErrorCodes.PrescriptionTemplate.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            return Result.Failure<PrescriptionTemplateItem>(ErrorCodes.PrescriptionTemplate.InvalidMedication);
        }

        if (medicationName.Length > 200)
        {
            return Result.Failure<PrescriptionTemplateItem>(ErrorCodes.PrescriptionTemplate.InvalidMedication);
        }

        return Result.Success(new PrescriptionTemplateItem(
            id,
            templateId,
            medicationName.Trim(),
            concentration.Trim(),
            dose.Trim(),
            route.Trim(),
            frequency.Trim(),
            duration.Trim(),
            instructions.Trim(),
            sortOrder));
    }
}
