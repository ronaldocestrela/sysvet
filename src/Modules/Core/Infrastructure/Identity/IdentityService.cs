using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Bridges ASP.NET Core Identity to application auth use cases.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticatedUserDto>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidCredentials);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.LockedOut);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.LockedOut);
        }

        if (!signInResult.Succeeded)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Result.Success(new AuthenticatedUserDto(user.Id, user.Email ?? string.Empty, user.TenantId, roles.ToList()));
    }

    /// <inheritdoc />
    public async Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        string role,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return Result.Failure<string>(ErrorCodes.Auth.DuplicateEmail);
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            TenantId = tenantId
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var duplicate = createResult.Errors.Any(e => e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
            return Result.Failure<string>(duplicate ? ErrorCodes.Auth.DuplicateEmail : ErrorCodes.Validation.Error);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Failure<string>(ErrorCodes.Auth.InvalidRole);
        }

        return Result.Success(user.Id);
    }
}
