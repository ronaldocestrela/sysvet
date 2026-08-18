using System.Security.Claims;
using Core.Domain;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

public static class AuthEndpointsExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/login", async ([FromBody] LoginRequest request, UserManager<AppUser> userManager, ITokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.GenerateAccessToken(user, roles);
            var refreshToken = tokenService.GenerateRefreshToken();

            // Store refresh token in user logic can be added here...
            user.SecurityStamp = refreshToken; // Simulating storing refresh token (in real world use a specific table or field)
            await userManager.UpdateAsync(user);

            return Results.Ok(new LoginResponse { AccessToken = token, RefreshToken = refreshToken });
        });

        group.MapPost("/refresh", async ([FromBody] RefreshRequest request, UserManager<AppUser> userManager, ITokenService tokenService) =>
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) return Results.Unauthorized();

            var email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.FindByEmailAsync(email!);

            if (user == null || user.SecurityStamp != request.RefreshToken)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            user.SecurityStamp = newRefreshToken;
            await userManager.UpdateAsync(user);

            return Results.Ok(new LoginResponse { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = user.FindFirstValue(ClaimTypes.Email),
                TenantId = user.FindFirstValue("TenantId"),
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
            });
        }).RequireAuthorization();

        return builder;
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
