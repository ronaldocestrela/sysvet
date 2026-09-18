namespace Veterinary.Domain.Enums;

/// <summary>Lifecycle of an inpatient medication order.</summary>
public enum HospitalMedicationOrderStatus
{
    Active = 1,
    Suspended = 2,
    Stopped = 3
}
