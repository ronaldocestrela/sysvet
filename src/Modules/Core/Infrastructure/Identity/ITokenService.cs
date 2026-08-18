using System.Security.Claims;

namespace Core.Infrastructure.Identity;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
