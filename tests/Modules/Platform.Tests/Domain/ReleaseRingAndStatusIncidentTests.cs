using FluentAssertions;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class ReleaseRingAndStatusIncidentTests
{
    [Fact]
    public void Tenant_SetReleaseRing_OnDeletedTenant_Fails()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "ring-test", "Ring Test").Value;
        tenant.MarkDeleted().IsSuccess.Should().BeTrue();

        tenant.SetReleaseRing(ReleaseRing.Canary).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Tenant_Create_DefaultsToGeneralAvailability()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "ga-default", "GA Default").Value;
        tenant.ReleaseRing.Should().Be(ReleaseRing.GeneralAvailability);
    }

    [Fact]
    public void StatusIncident_Resolve_Twice_Fails()
    {
        var opened = StatusIncident.Open("Sync delay", StatusIncidentImpact.Minor, "sync", DateTimeOffset.UtcNow);
        opened.IsSuccess.Should().BeTrue();
        var incident = opened.Value;
        incident.Resolve(DateTimeOffset.UtcNow.AddMinutes(5)).IsSuccess.Should().BeTrue();
        incident.Resolve(DateTimeOffset.UtcNow.AddMinutes(10)).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void StatusIncident_Open_WithEmptyComponents_Fails()
    {
        var result = StatusIncident.Open("Outage", StatusIncidentImpact.Major, " ", DateTimeOffset.UtcNow);
        result.IsFailure.Should().BeTrue();
    }
}
