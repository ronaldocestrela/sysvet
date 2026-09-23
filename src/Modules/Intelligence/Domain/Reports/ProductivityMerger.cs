namespace Intelligence.Domain.Reports;

/// <summary>Merges cross-module productivity counters by staff user (roadmap 10.3).</summary>
public static class ProductivityMerger
{
    /// <summary>Sums sales, clinical and grooming metrics per user without double-counting revenue from appointments.</summary>
    public static IReadOnlyList<ProductivityRow> Merge(IEnumerable<ProductivityContribution> contributions)
    {
        var map = new Dictionary<Guid, ProductivityRow>();

        foreach (var contribution in contributions)
        {
            if (contribution.UserId == Guid.Empty)
            {
                continue;
            }

            if (!map.TryGetValue(contribution.UserId, out var existing))
            {
                map[contribution.UserId] = new ProductivityRow(
                    contribution.UserId,
                    contribution.SalesNetAmount,
                    contribution.SalesQuantity,
                    contribution.ClinicalCompleted,
                    contribution.GroomingCompleted);
                continue;
            }

            map[contribution.UserId] = existing with
            {
                SalesNetAmount = existing.SalesNetAmount + contribution.SalesNetAmount,
                SalesQuantity = existing.SalesQuantity + contribution.SalesQuantity,
                ClinicalCompleted = existing.ClinicalCompleted + contribution.ClinicalCompleted,
                GroomingCompleted = existing.GroomingCompleted + contribution.GroomingCompleted
            };
        }

        return map.Values
            .OrderByDescending(r => r.SalesNetAmount + r.ClinicalCompleted + r.GroomingCompleted)
            .ThenBy(r => r.UserId)
            .ToList();
    }
}

/// <summary>Partial productivity metrics from one source module.</summary>
public sealed record ProductivityContribution(
    Guid UserId,
    decimal SalesNetAmount,
    decimal SalesQuantity,
    int ClinicalCompleted,
    int GroomingCompleted);

/// <summary>Merged productivity row for one professional.</summary>
public sealed record ProductivityRow(
    Guid UserId,
    decimal SalesNetAmount,
    decimal SalesQuantity,
    int ClinicalCompleted,
    int GroomingCompleted);
