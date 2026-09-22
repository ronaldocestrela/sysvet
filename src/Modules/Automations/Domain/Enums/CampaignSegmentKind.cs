namespace Automations.Domain.Enums;

/// <summary>
/// Built-in audience definitions for marketing and NPS campaigns (Fase 8.3).
/// </summary>
public enum CampaignSegmentKind
{
    /// <summary>Tutors whose last completed visit is older than <see cref="Entities.Campaign.InactiveDays"/>.</summary>
    Inactive90Days = 1,

    /// <summary>NPS survey after a completed clinical or grooming appointment on the local day.</summary>
    PostAppointment = 2
}
