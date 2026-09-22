using Automations.Domain.Services;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class MarketingMessageClassifierTests
{
    [Theory]
    [InlineData("campaign.inactive", true)]
    [InlineData("nps.request", true)]
    [InlineData("reminder.vaccine", false)]
    public void RequiresMarketingConsent_ClassifiesTemplates(string code, bool expected)
    {
        MarketingMessageClassifier.RequiresMarketingConsent(code).Should().Be(expected);
    }
}
