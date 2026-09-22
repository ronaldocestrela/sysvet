using Automations.Domain.Entities;
using Automations.Domain.Enums;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class CampaignTests
{
    [Fact]
    public void Create_InactiveSegment_UsesDefaultTemplate()
    {
        var result = Campaign.Create("Reativação", CampaignSegmentKind.Inactive90Days);
        result.IsSuccess.Should().BeTrue();
        result.Value.TemplateCode.Should().Be("campaign.inactive");
        result.Value.Status.Should().Be(CampaignStatus.Draft);
    }

    [Fact]
    public void Create_PostAppointment_UsesNpsTemplate()
    {
        var result = Campaign.Create("NPS pós-atendimento", CampaignSegmentKind.PostAppointment);
        result.IsSuccess.Should().BeTrue();
        result.Value.TemplateCode.Should().Be("nps.request");
    }

    [Fact]
    public void Create_EmptyName_Fails()
    {
        Campaign.Create(" ", CampaignSegmentKind.Inactive90Days).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RecordRun_AddsMetrics()
    {
        var campaign = Campaign.Create("X", CampaignSegmentKind.Inactive90Days).Value;
        var run = campaign.RecordRun(audienceCount: 10, enqueuedCount: 8, DateTimeOffset.UtcNow);
        run.EnqueuedCount.Should().Be(8);
        campaign.Runs.Should().HaveCount(1);
    }
}
