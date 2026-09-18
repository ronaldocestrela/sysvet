namespace Veterinary.Domain.Enums;

/// <summary>Status of a scheduled medication administration slot.</summary>
public enum MedicationAdministrationStatus
{
    Pending = 1,
    Administered = 2,
    Skipped = 3,
    Cancelled = 4
}
