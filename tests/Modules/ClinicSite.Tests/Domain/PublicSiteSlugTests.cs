using ClinicSite.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicSite.Tests.Domain;

public class PublicSiteSlugTests
{
    [Theory]
    [InlineData("Minha-Clinica", "minha-clinica")]
    [InlineData("  PET SHOP  ", "pet-shop")]
    public void Normalize_ShouldLowercaseAndTrim(string input, string expected)
    {
        PublicSiteSlug.Normalize(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("clinica-pet")]
    [InlineData("a1b2c3")]
    public void IsValid_ShouldAcceptValidSlugs(string slug)
    {
        PublicSiteSlug.IsValid(slug).Should().BeTrue();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("-invalid")]
    [InlineData("invalid-")]
    [InlineData("")]
    public void IsValid_ShouldRejectInvalidSlugs(string slug)
    {
        PublicSiteSlug.IsValid(slug).Should().BeFalse();
    }
}
