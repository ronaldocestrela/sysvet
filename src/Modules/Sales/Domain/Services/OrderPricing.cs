namespace Sales.Domain.Services;

/// <summary>
/// Order-level discount and net line allocation helpers.
/// </summary>
public static class OrderPricing
{
    /// <summary>Computes discount currency amount from subtotal and percent.</summary>
    public static decimal ComputeDiscountAmount(decimal subtotal, decimal discountPercent)
    {
        if (discountPercent <= 0 || subtotal <= 0)
        {
            return 0m;
        }

        return Math.Round(subtotal * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Net line share after proportional order discount.</summary>
    public static decimal ComputeLineNet(decimal lineGross, decimal subtotal, decimal discountAmount)
    {
        if (subtotal <= 0)
        {
            return lineGross;
        }

        var share = lineGross / subtotal;
        return Math.Round(lineGross - share * discountAmount, 2, MidpointRounding.AwayFromZero);
    }
}
