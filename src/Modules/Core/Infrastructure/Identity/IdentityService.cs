using Core.Application.Auth.Dtos;
using Core.Application.Authorization;
using Core.Application.Common;
using Core.Application.Common.Interfaces;
using Core.Application.Users.Dtos;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Bridges ASP.NET Core Identity to application auth and staff user use cases.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IAccessProfileSeeder _accessProfileSeeder;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSignInGate _tenantSignInGate;

    public IdentityService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IAccessProfileRepository accessProfileRepository,
        IAccessProfileSeeder accessProfileSeeder,
        ITenantContext tenantContext,
        ITenantSignInGate tenantSignInGate)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _accessProfileRepository = accessProfileRepository;
        _accessProfileSeeder = accessProfileSeeder;
        _tenantContext = tenantContext;
        _tenantSignInGate = tenantSignInGate;
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

        if (user.IsDisabled)
        {
            return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.Disabled);
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

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var isSuperAdmin = roles.Contains(ApplicationRoles.SuperAdmin);
        var isTutorOnly = roles.Count == 1 && roles[0] == ApplicationRoles.Tutor;
        if (!isTutorOnly && !isSuperAdmin && user.TenantId != Guid.Empty)
        {
            var tenantActive = await _tenantSignInGate.EnsureActiveAsync(user.TenantId, cancellationToken);
            if (tenantActive.IsFailure)
            {
                return Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.TenantNotActive);
            }
        }

        if (!isTutorOnly && !isSuperAdmin)
        {
            var primaryRole = roles.FirstOrDefault(r => r != ApplicationRoles.Tutor) ?? ApplicationRoles.Receptionist;
            if (user.AccessProfileId == Guid.Empty)
            {
                await EnsureAccessProfileAsync(user.Id, user.TenantId, primaryRole, cancellationToken);
                user = await _userManager.FindByIdAsync(user.Id) ?? user;
            }
        }

        return Result.Success(new AuthenticatedUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.TenantId,
            user.AccessProfileId,
            roles));
    }

    /// <inheritdoc />
    public async Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        string role,
        Guid tenantId,
        Guid accessProfileId,
        string? displayName = null,
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
            TenantId = tenantId,
            AccessProfileId = accessProfileId,
            DisplayName = displayName?.Trim()
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

    /// <inheritdoc />
    public async Task<Result<string>> CreateTutorUserAsync(
        string email,
        string password,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await CreateUserAsync(
            email,
            password,
            ApplicationRoles.Tutor,
            tenantId,
            accessProfileId: Guid.Empty,
            displayName: null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<StaffUserDto>> GetByIdAsync(string userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await FindTenantUserAsync(userId, tenantId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<StaffUserDto>(ErrorCodes.UserAccount.NotFound);
        }

        return Result.Success(await MapStaffUserAsync(user, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<StaffUserDto>>> ListByTenantAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? nameOrEmailFilter,
        CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.Where(u => u.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(nameOrEmailFilter))
        {
            var filter = nameOrEmailFilter.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(filter))
                || (u.DisplayName != null && u.DisplayName.Contains(filter)));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<StaffUserDto>();
        foreach (var user in users)
        {
            items.Add(await MapStaffUserAsync(user, cancellationToken));
        }

        return Result.Success(new PagedResult<StaffUserDto>(items, page, pageSize, total));
    }

    /// <inheritdoc />
    public async Task<Result> UpdateProfileAndNameAsync(
        string userId,
        Guid tenantId,
        Guid accessProfileId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        var user = await FindTenantUserAsync(userId, tenantId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorCodes.UserAccount.NotFound);
        }

        var profile = await _accessProfileRepository.GetByIdAsync(accessProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ErrorCodes.UserAccount.ProfileNotFound);
        }

        user.AccessProfileId = accessProfileId;
        user.DisplayName = displayName?.Trim();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure(ErrorCodes.Validation.Error);
        }

        return await ReplaceRoleAsync(userId, profile.BaseRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> SetDisabledAsync(string userId, Guid tenantId, bool disabled, CancellationToken cancellationToken = default)
    {
        var user = await FindTenantUserAsync(userId, tenantId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorCodes.UserAccount.NotFound);
        }

        user.IsDisabled = disabled;
        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded ? Result.Success() : Result.Failure(ErrorCodes.Validation.Error);
    }

    /// <inheritdoc />
    public async Task<Result> ResetPasswordAsync(string userId, Guid tenantId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await FindTenantUserAsync(userId, tenantId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorCodes.UserAccount.NotFound);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return resetResult.Succeeded ? Result.Success() : Result.Failure(ErrorCodes.Validation.Error);
    }

    /// <inheritdoc />
    public async Task<Result> ReplaceRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(ErrorCodes.UserAccount.NotFound);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return Result.Failure(ErrorCodes.Validation.Error);
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        return addResult.Succeeded ? Result.Success() : Result.Failure(ErrorCodes.Auth.InvalidRole);
    }

    /// <inheritdoc />
    public async Task<int> CountAdminsInTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        return admins.Count(u => u.TenantId == tenantId && !u.IsDisabled);
    }

    /// <inheritdoc />
    public async Task<int> CountUsersWithProfileAsync(Guid accessProfileId, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.CountAsync(u => u.AccessProfileId == accessProfileId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> EnsureAccessProfileAsync(
        string userId,
        Guid tenantId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = await FindTenantUserAsync(userId, tenantId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(ErrorCodes.UserAccount.NotFound);
        }

        if (user.AccessProfileId != Guid.Empty)
        {
            return Result.Success(user.AccessProfileId);
        }

        await WithTenantScopeAsync(tenantId, async () =>
        {
            await _accessProfileSeeder.EnsureTenantProfilesAsync(tenantId, cancellationToken);
        });

        var profile = await WithTenantScopeAsync(tenantId, () =>
            _accessProfileRepository.GetSystemProfileByBaseRoleAsync(role, cancellationToken));
        if (profile is null)
        {
            return Result.Failure<Guid>(ErrorCodes.UserAccount.ProfileNotFound);
        }

        user.AccessProfileId = profile.Id;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return Result.Failure<Guid>(ErrorCodes.Validation.Error);
        }

        return Result.Success(profile.Id);
    }

    private async Task<AppUser?> FindTenantUserAsync(string userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.TenantId != tenantId)
        {
            return null;
        }

        return user;
    }

    private async Task WithTenantScopeAsync(Guid tenantId, Func<Task> action)
    {
        var previous = _tenantContext.TenantId;
        _tenantContext.TenantId = tenantId;
        try
        {
            await action();
        }
        finally
        {
            _tenantContext.TenantId = previous;
        }
    }

    private async Task<T> WithTenantScopeAsync<T>(Guid tenantId, Func<Task<T>> action)
    {
        var previous = _tenantContext.TenantId;
        _tenantContext.TenantId = tenantId;
        try
        {
            return await action();
        }
        finally
        {
            _tenantContext.TenantId = previous;
        }
    }

    private async Task<StaffUserDto> MapStaffUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var profileName = string.Empty;
        if (user.AccessProfileId != Guid.Empty)
        {
            var profile = await _accessProfileRepository.GetByIdAsync(user.AccessProfileId, cancellationToken);
            profileName = profile?.Name ?? string.Empty;
        }

        return new StaffUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.TenantId,
            user.AccessProfileId,
            profileName,
            user.IsDisabled,
            roles);
    }
}
