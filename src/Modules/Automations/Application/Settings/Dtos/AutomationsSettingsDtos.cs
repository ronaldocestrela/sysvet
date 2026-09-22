namespace Automations.Application.Settings.Dtos;

/// <summary>
/// Tenant Automations settings exposed to the API.
/// </summary>
public sealed class AutomationsSettingsDto
{
    public string TimeZoneId { get; init; } = "America/Sao_Paulo";
    public string BusinessStart { get; init; } = "08:00";
    public string BusinessEnd { get; init; } = "18:00";
    public IReadOnlyList<string> BusinessDays { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Tutor channel opt-in flags.
/// </summary>
public sealed class TutorMessagingPreferenceDto
{
    public Guid TutorId { get; init; }
    public bool WhatsAppEnabled { get; init; }
    public bool EmailEnabled { get; init; }
    public bool MarketingEnabled { get; init; }
}
