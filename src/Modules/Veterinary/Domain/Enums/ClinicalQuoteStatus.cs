namespace Veterinary.Domain.Enums;

/// <summary>Workflow status for a clinical quote presented to the pet owner.</summary>
public enum ClinicalQuoteStatus
{
    /// <summary>Editable draft not yet shared with the tutor.</summary>
    Draft = 0,

    /// <summary>Sent to the tutor for approval decision.</summary>
    Sent = 1,

    /// <summary>Accepted by the tutor; lines are frozen and pending PDV conversion.</summary>
    Approved = 2,

    /// <summary>Declined by the tutor.</summary>
    Rejected = 3
}
