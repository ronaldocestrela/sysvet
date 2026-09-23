using FluentAssertions;
using Intelligence.Domain.Reports;

namespace Intelligence.Tests.Domain;

public class ProductivityMergerTests
{
    [Fact]
    public void Merge_CombinesModulesWithoutDuplicatingSalesRevenue()
    {
        var user = Guid.NewGuid();

        var rows = ProductivityMerger.Merge(
        [
            new ProductivityContribution(user, 100m, 2m, 0, 0),
            new ProductivityContribution(user, 0m, 0m, 3, 0),
            new ProductivityContribution(user, 0m, 0m, 0, 4)
        ]);

        rows.Should().ContainSingle();
        rows[0].SalesNetAmount.Should().Be(100m);
        rows[0].ClinicalCompleted.Should().Be(3);
        rows[0].GroomingCompleted.Should().Be(4);
    }

    [Fact]
    public void Merge_IgnoresEmptyUserId()
    {
        var rows = ProductivityMerger.Merge([new ProductivityContribution(Guid.Empty, 10m, 1m, 1, 1)]);
        rows.Should().BeEmpty();
    }
}
