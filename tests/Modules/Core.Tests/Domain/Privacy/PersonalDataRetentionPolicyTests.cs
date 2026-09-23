using Core.Domain.Privacy;
using FluentAssertions;

namespace Core.Tests.Domain.Privacy;

public class PersonalDataRetentionPolicyTests
{
    [Fact]
    public void IsEligibleForAutomaticAnonymization_ShouldBeFalse_WhenNotSoftDeleted()
    {
        var asOf = new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);

        PersonalDataRetentionPolicy.IsEligibleForAutomaticAnonymization(
                isDeleted: false,
                deletedAt: null,
                isAnonymized: false,
                asOf)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsEligibleForAutomaticAnonymization_ShouldBeTrue_WhenSoftDeletedOverFiveYears()
    {
        var deletedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var asOf = deletedAt.Add(PersonalDataRetentionPolicy.SoftDeletedRetentionBeforeAnonymization).AddDays(1);

        PersonalDataRetentionPolicy.IsEligibleForAutomaticAnonymization(
                isDeleted: true,
                deletedAt: deletedAt,
                isAnonymized: false,
                asOf)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsEligibleForAutomaticAnonymization_ShouldBeFalse_WhenAlreadyAnonymized()
    {
        var deletedAt = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var asOf = new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);

        PersonalDataRetentionPolicy.IsEligibleForAutomaticAnonymization(
                isDeleted: true,
                deletedAt: deletedAt,
                isAnonymized: true,
                asOf)
            .Should()
            .BeFalse();
    }
}
