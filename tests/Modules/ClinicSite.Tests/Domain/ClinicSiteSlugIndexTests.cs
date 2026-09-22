using ClinicSite.Domain;
using ClinicSite.Domain.Entities;
using FluentAssertions;

namespace ClinicSite.Tests.Domain;

public class ClinicSiteSlugIndexTests
{
    [Fact]
    public void Create_WithValidSlug_ShouldNormalize()
    {
        var tenantId = Guid.NewGuid();
        var result = ClinicSiteSlugIndex.Create("Minha-Clinica", tenantId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be("minha-clinica");
        result.Value.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Create_WithInvalidSlug_ShouldFail()
    {
        var result = ClinicSiteSlugIndex.Create("ab", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Site.InvalidSlug.Code);
    }
}
