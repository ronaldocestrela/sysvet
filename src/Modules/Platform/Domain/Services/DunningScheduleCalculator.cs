using Platform.Domain.Entities;

namespace Platform.Domain.Services;

/// <summary>Computes due dunning notice steps from past-due start (9.5).</summary>
public static class DunningScheduleCalculator
{
    /// <summary>Default day offsets for notices (day 0 = immediate).</summary>
    public static readonly int[] DefaultNoticeDays = { 0, 3, 7 };

    /// <summary>Default channels per step (email + SMS).</summary>
    public static readonly DunningChannel[] DefaultChannels =
    {
        DunningChannel.Email,
        DunningChannel.Sms
    };

    /// <summary>
    /// Returns notice steps that are due at <paramref name="asOfUtc"/> and not yet sent.
    /// </summary>
    public static IReadOnlyList<(int StepDay, DunningChannel Channel)> GetDueSteps(
        DateTimeOffset pastDueSince,
        DateTimeOffset asOfUtc,
        IReadOnlySet<string> sentKeys,
        IReadOnlyList<int>? noticeDays = null)
    {
        noticeDays ??= DefaultNoticeDays;
        var due = new List<(int, DunningChannel)>();

        foreach (var day in noticeDays)
        {
            var scheduledAt = pastDueSince.AddDays(day);
            if (asOfUtc < scheduledAt)
            {
                continue;
            }

            foreach (var channel in DefaultChannels)
            {
                var key = BuildKey(day, channel);
                if (!sentKeys.Contains(key))
                {
                    due.Add((day, channel));
                }
            }
        }

        return due;
    }

    /// <summary>Builds idempotency fragment for a step/channel pair.</summary>
    public static string BuildKey(int stepDay, DunningChannel channel) => $"{stepDay}:{(int)channel}";
}
