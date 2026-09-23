namespace Core.Domain.Privacy;

/// <summary>
/// LGPD retention rules for tutor personal data after operational soft delete.
/// </summary>
public static class PersonalDataRetentionPolicy
{
    /// <summary>
    /// Duration after soft delete before scheduled anonymization may run (see runbook).
    /// </summary>
    public static readonly TimeSpan SoftDeletedRetentionBeforeAnonymization = TimeSpan.FromDays(365 * 5);

    /// <summary>
    /// Returns whether a tutor is eligible for automatic anonymization at <paramref name="asOf"/>.
    /// </summary>
    public static bool IsEligibleForAutomaticAnonymization(
        bool isDeleted,
        DateTimeOffset? deletedAt,
        bool isAnonymized,
        DateTimeOffset asOf)
    {
        if (isAnonymized)
        {
            return false;
        }

        if (!isDeleted || deletedAt is null)
        {
            return false;
        }

        return asOf - deletedAt.Value >= SoftDeletedRetentionBeforeAnonymization;
    }
}
