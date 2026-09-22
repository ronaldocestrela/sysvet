using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Per-tutor channel and marketing opt-out flags (defaults to all enabled when absent).
/// </summary>
public sealed class TutorMessagingPreference : AggregateRoot
{
    public Guid TutorId { get; private set; }
    public bool WhatsAppEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool MarketingEnabled { get; private set; }

    private TutorMessagingPreference() { }

    /// <summary>
    /// Creates preferences with explicit channel toggles.
    /// </summary>
    public static Result<TutorMessagingPreference> Create(
        Guid tutorId,
        bool whatsAppEnabled,
        bool emailEnabled,
        bool marketingEnabled = true,
        Guid? id = null)
    {
        if (tutorId == Guid.Empty)
        {
            return Result.Failure<TutorMessagingPreference>(ErrorCodes.Preference.InvalidTutor);
        }

        return Result.Success(new TutorMessagingPreference
        {
            Id = id ?? Guid.NewGuid(),
            TutorId = tutorId,
            WhatsAppEnabled = whatsAppEnabled,
            EmailEnabled = emailEnabled,
            MarketingEnabled = marketingEnabled
        });
    }

    /// <summary>
    /// Updates channel and marketing toggles.
    /// </summary>
    public Result Update(bool whatsAppEnabled, bool emailEnabled, bool marketingEnabled)
    {
        WhatsAppEnabled = whatsAppEnabled;
        EmailEnabled = emailEnabled;
        MarketingEnabled = marketingEnabled;
        return Result.Success();
    }

    /// <summary>
    /// Effective preference when no row exists (all channels and marketing on).
    /// </summary>
    public static TutorMessagingPreference DefaultFor(Guid tutorId) =>
        Create(tutorId, whatsAppEnabled: true, emailEnabled: true, marketingEnabled: true).Value;
}
