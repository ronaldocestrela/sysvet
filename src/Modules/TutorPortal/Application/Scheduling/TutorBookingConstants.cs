namespace TutorPortal.Application.Scheduling;

/// <summary>
/// Shared constants for tutor portal self-scheduling (8.6).
/// </summary>
public static class TutorBookingConstants
{
    /// <summary>Sentinel service id for the virtual clinical consultation offering.</summary>
    public static readonly Guid ClinicalConsultationServiceId = new("00000000-0000-4000-8000-000000000001");

    /// <summary>Default clinical consultation duration aligned with staff agenda UI.</summary>
    public const int ClinicalConsultationDurationMinutes = 30;

    /// <summary>Display name for the virtual clinical service.</summary>
    public const string ClinicalConsultationName = "Consulta clínica";

    /// <summary>Reason/notes stored on appointments created via the tutor portal.</summary>
    public const string PortalBookingText = "Agendamento pelo portal";
}
