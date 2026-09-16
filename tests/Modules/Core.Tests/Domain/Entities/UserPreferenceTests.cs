using Core.Domain.Entities;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class UserPreferenceTests
{
    [Fact]
    public void Update_ShouldFail_WhenPayloadExceedsMaxSize()
    {
        var pref = UserPreference.Create("user-1").Value;
        var huge = new string('x', UserPreference.MaxJsonLength + 1);

        var result = pref.Update(huge, "{}", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserPreference.PayloadTooLarge");
    }
}
