namespace Veterinary.Domain.Enums;

/// <summary>Lifecycle of a clinical exam request.</summary>
public enum ClinicalExamStatus
{
    Requested = 1,
    Completed = 2,
    Cancelled = 3
}
