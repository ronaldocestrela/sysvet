using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Core.Application.Auth.Dtos;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Core.Tests.Infrastructure.Identity;

public class JwtAccessTokenIssuerTests
{
    private readonly JwtSettings _settings = new()
    {
        Secret = "this-is-a-super-secret-key-that-needs-to-be-long-enough-for-hs256",
        Issuer = "sysvet-api",
        Audience = "sysvet-clients",
        ExpiryMinutes = 60,
        RefreshExpiryDays = 7
    };

    [Fact]
    public void IssueAccessToken_ShouldIncludeRoleAndTenantClaims()
    {
        var issuer = new JwtAccessTokenIssuer(Microsoft.Extensions.Options.Options.Create(_settings));
        var tenantId = Guid.NewGuid();
        var user = new AuthenticatedUserDto("user-1", "vet@sysvet.com", tenantId, Guid.NewGuid(), ["Veterinarian"]);

        var token = issuer.IssueAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-1");
        jwt.Claims.Should().Contain(c => c.Type == "TenantId" && c.Value == tenantId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Veterinarian");
        issuer.AccessTokenLifetimeSeconds.Should().Be(3600);
    }
}
