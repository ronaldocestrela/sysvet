using ClinicSite.Domain;
using ClinicSite.Domain.Entities;
using FluentAssertions;

namespace ClinicSite.Tests.Domain;

public class ClinicSiteProfileTests
{
    [Fact]
    public void Publish_WithoutContact_ShouldFail()
    {
        var profile = ClinicSiteProfile.CreateDefault().Value;
        profile.SetSlug("minha-clinica");
        profile.UpdateProfile(
            "Clínica Pet",
            null,
            "Rua A",
            "1",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001000",
            "",
            "",
            null,
            null);

        var result = profile.Publish();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Site.PublishIncomplete.Code);
    }

    [Fact]
    public void Publish_WithSlugNameAndPhone_ShouldSucceed()
    {
        var profile = ClinicSiteProfile.CreateDefault().Value;
        profile.SetSlug("minha-clinica");
        profile.UpdateProfile(
            "Clínica Pet",
            "Cuidamos do seu pet",
            "Rua A",
            "100",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001000",
            "11999998888",
            "",
            null,
            null);

        var result = profile.Publish();

        result.IsSuccess.Should().BeTrue();
        profile.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_ShouldClearPublishedFlag()
    {
        var profile = ClinicSiteProfile.CreateDefault().Value;
        profile.SetSlug("minha-clinica");
        profile.UpdateProfile(
            "Clínica Pet",
            null,
            "Rua A",
            "1",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01001000",
            "",
            "contato@clinica.com",
            null,
            null);
        profile.Publish().IsSuccess.Should().BeTrue();

        profile.Unpublish().IsSuccess.Should().BeTrue();
        profile.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void SetSlug_WithInvalidValue_ShouldFail()
    {
        var profile = ClinicSiteProfile.CreateDefault().Value;

        var result = profile.SetSlug("x");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Site.InvalidSlug.Code);
    }
}
