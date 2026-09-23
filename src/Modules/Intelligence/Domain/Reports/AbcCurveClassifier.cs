namespace Intelligence.Domain.Reports;

/// <summary>
/// Pure Pareto ABC classification ordered by descending net value (roadmap 10.3).
/// </summary>
public static class AbcCurveClassifier
{
    /// <summary>
    /// Classifies rows by value share. Tie-breaker is <paramref name="orderKey"/> ascending.
    /// Class uses cumulative share <em>before</em> the current item: A &lt; 80%, B &lt; 95%, else C.
    /// </summary>
    public static IReadOnlyList<AbcCurveItem> Classify(IEnumerable<AbcCurveInputRow> rows)
    {
        var ordered = rows
            .OrderByDescending(r => r.Value)
            .ThenBy(r => r.OrderKey, StringComparer.Ordinal)
            .ToList();

        if (ordered.Count == 0)
        {
            return Array.Empty<AbcCurveItem>();
        }

        var total = ordered.Sum(r => r.Value);
        if (total <= 0)
        {
            return Array.Empty<AbcCurveItem>();
        }

        var result = new List<AbcCurveItem>(ordered.Count);
        decimal cumulativeBefore = 0;
        var rank = 1;

        foreach (var row in ordered)
        {
            var share = decimal.Round(row.Value / total * 100m, 4, MidpointRounding.AwayFromZero);
            var cumulativeShare = decimal.Round((cumulativeBefore + row.Value) / total * 100m, 4, MidpointRounding.AwayFromZero);
            var abcClass = ClassifyCumulativeBefore(cumulativeBefore / total * 100m);

            result.Add(new AbcCurveItem(
                row.Id,
                row.Value,
                share,
                cumulativeShare,
                abcClass,
                rank++));

            cumulativeBefore += row.Value;
        }

        return result;
    }

    private static AbcClass ClassifyCumulativeBefore(decimal cumulativePercentBefore)
    {
        if (cumulativePercentBefore < 80m)
        {
            return AbcClass.A;
        }

        if (cumulativePercentBefore < 95m)
        {
            return AbcClass.B;
        }

        return AbcClass.C;
    }
}

/// <summary>Single row before ABC classification.</summary>
/// <param name="Id">Entity identifier (customer, product).</param>
/// <param name="Value">Net amount used for the curve.</param>
/// <param name="OrderKey">Stable tie-breaker when values are equal.</param>
public sealed record AbcCurveInputRow(Guid Id, decimal Value, string OrderKey);

/// <summary>Ranked ABC row after classification.</summary>
public sealed record AbcCurveItem(
    Guid Id,
    decimal Value,
    decimal SharePercent,
    decimal CumulativeSharePercent,
    AbcClass Class,
    int Rank);
