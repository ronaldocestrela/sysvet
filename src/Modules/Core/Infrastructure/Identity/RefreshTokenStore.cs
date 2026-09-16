using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.Identity;

/// <summary>
/// EF-backed refresh token persistence with rotation and reuse detection.
/// </summary>
public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly CoreDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenStore(
        CoreDbContext dbContext,
        UserManager<AppUser> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public async Task<string> IssueAsync(string userId, CancellationToken cancellationToken = default)
    {
        var plain = GeneratePlainToken();
        var entity = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = RefreshTokenHasher.Hash(plain),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiryDays)
        };

        _dbContext.UserRefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return plain;
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticatedUserDto>> RotateAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = RefreshTokenHasher.Hash(refreshToken);
        var stored = await _dbContext.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidRefreshToken);
        }

        if (stored.RevokedAtUtc is not null)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidRefreshToken);
        }

        if (stored.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidRefreshToken);
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user is null)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidRefreshToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new AuthenticatedUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.TenantId,
            user.AccessProfileId,
            roles.ToList());
        return Result.Success(dto);
    }

    private static string GeneratePlainToken()
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
