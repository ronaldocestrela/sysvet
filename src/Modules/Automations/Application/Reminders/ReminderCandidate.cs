namespace Automations.Application.Reminders;

/// <summary>
/// A tutor-facing reminder ready to be planned into outbound jobs.
/// </summary>
public sealed record ReminderCandidate(
    ReminderKind Kind,
    Guid TutorId,
    Guid? PetId,
    Guid SourceId,
    string TemplateCode,
    string IdempotencyKeyBase,
    IReadOnlyDictionary<string, string> Tokens);
