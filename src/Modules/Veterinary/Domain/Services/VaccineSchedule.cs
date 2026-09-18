namespace Veterinary.Domain.Services;

/// <summary>Classification of a vaccine due date relative to clinic horizon.</summary>
public enum VaccineAlertKind
{
    None = 0,
    Overdue = 1,
    Upcoming = 2
}

/// <summary>Pure domain helpers for next-dose scheduling and alert classification.</summary>
public static class VaccineSchedule
{
    /// <summary>Computes the next due instant from application date and protocol interval.</summary>
    public static DateTimeOffset? ComputeNextDueDate(DateTimeOffset appliedAt, int? nextDoseIntervalInDays)
    {
        if (nextDoseIntervalInDays is null or <= 0)
        {
            return null;
        }

        return appliedAt.AddDays(nextDoseIntervalInDays.Value);
    }

    /// <summary>Classifies a due date against UTC now and an upcoming horizon (inclusive).</summary>
    public static VaccineAlertKind ClassifyAlert(DateTimeOffset? nextDueDate, DateTimeOffset utcNow, int horizonDays)
    {
        if (nextDueDate is null)
        {
            return VaccineAlertKind.None;
        }

        if (nextDueDate.Value < utcNow)
        {
            return VaccineAlertKind.Overdue;
        }

        var horizonEnd = utcNow.AddDays(horizonDays);
        if (nextDueDate.Value <= horizonEnd)
        {
            return VaccineAlertKind.Upcoming;
        }

        return VaccineAlertKind.None;
    }

    /// <summary>Returns whole days since birth at the reference date, or null when birth date is unknown.</summary>
    public static int? GetAgeInDays(DateOnly? birthDate, DateOnly referenceDate)
    {
        if (birthDate is null)
        {
            return null;
        }

        return referenceDate.DayNumber - birthDate.Value.DayNumber;
    }

    /// <summary>Checks whether pet age falls within protocol dose age bounds.</summary>
    public static bool IsAgeEligible(int? ageInDays, int minAgeInDays, int? maxAgeInDays)
    {
        if (ageInDays is null)
        {
            return true;
        }

        if (ageInDays.Value < minAgeInDays)
        {
            return false;
        }

        if (maxAgeInDays is not null && ageInDays.Value > maxAgeInDays.Value)
        {
            return false;
        }

        return true;
    }
}
