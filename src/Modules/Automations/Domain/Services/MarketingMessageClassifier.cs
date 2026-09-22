namespace Automations.Domain.Services;

/// <summary>
/// Distinguishes marketing/NPS jobs from transactional reminders for opt-out rules.
/// </summary>
public static class MarketingMessageClassifier
{
    /// <summary>
    /// Returns true when the job requires tutor marketing consent in addition to channel opt-in.
    /// </summary>
    public static bool RequiresMarketingConsent(string templateCode) =>
        templateCode.StartsWith("campaign.", StringComparison.OrdinalIgnoreCase)
        || templateCode.StartsWith("nps.", StringComparison.OrdinalIgnoreCase);
}
