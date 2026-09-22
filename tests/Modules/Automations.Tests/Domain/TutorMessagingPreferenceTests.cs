using Automations.Domain.Entities;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class TutorMessagingPreferenceTests
{
    [Fact]
    public void DefaultFor_EnablesAllChannels()
    {
        var tutorId = Guid.NewGuid();
        var pref = TutorMessagingPreference.DefaultFor(tutorId);
        pref.WhatsAppEnabled.Should().BeTrue();
        pref.EmailEnabled.Should().BeTrue();
        pref.MarketingEnabled.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTutor_Fails()
    {
        var result = TutorMessagingPreference.Create(Guid.Empty, true, true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_PersistsMarketingFlag()
    {
        var pref = TutorMessagingPreference.Create(Guid.NewGuid(), true, true, marketingEnabled: true).Value;
        pref.Update(true, false, false);
        pref.MarketingEnabled.Should().BeFalse();
    }
}
