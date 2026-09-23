namespace Platform.Domain.Services;

/// <summary>
/// Mid-cycle plan or add-on price delta for SaaS billing (9.3; charged in 9.4).
/// </summary>
public static class ProrationCalculator
{
    /// <summary>Default billing period length in days.</summary>
    public const int DefaultPeriodDays = 30;

    /// <summary>
    /// Returns signed delta: positive amount to charge, negative amount to credit.
    /// </summary>
    public static decimal CalculateDelta(decimal oldMonthlyPrice, decimal newMonthlyPrice, int daysRemaining, int periodDays = DefaultPeriodDays)
    {
        if (periodDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(periodDays));
        }

        daysRemaining = Math.Clamp(daysRemaining, 0, periodDays);
        return (newMonthlyPrice - oldMonthlyPrice) * daysRemaining / periodDays;
    }

    /// <summary>Remaining whole days in the current period (inclusive of start day).</summary>
    public static int DaysRemaining(DateTimeOffset periodStart, DateTimeOffset periodEnd, DateTimeOffset asOfUtc)
    {
        if (asOfUtc >= periodEnd)
        {
            return 0;
        }

        if (asOfUtc <= periodStart)
        {
            return (int)Math.Ceiling((periodEnd - periodStart).TotalDays);
        }

        return (int)Math.Ceiling((periodEnd - asOfUtc).TotalDays);
    }
}
