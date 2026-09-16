using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Resolves permissions from the authenticated user's access profile.
/// </summary>
public sealed class PermissionChecker : IPermissionChecker
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly ITenantContext _tenantContext;

    public PermissionChecker(
        ICurrentUser currentUser,
        IAccessProfileRepository accessProfileRepository,
        ITenantContext tenantContext)
    {
        _currentUser = currentUser;
        _accessProfileRepository = accessProfileRepository;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default)
    {
        var permissions = await GetGrantedPermissionsAsync(cancellationToken);
        return permissions.Contains(permissionCode, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetGrantedPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return [];
        }

        var tenantId = _currentUser.TenantId != Guid.Empty ? _currentUser.TenantId : _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return [];
        }

        var profile = await ResolveProfileAsync(tenantId, cancellationToken);
        return profile?.PermissionCodes.ToList() ?? [];
    }

    private async Task<AccessProfile?> ResolveProfileAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (_currentUser.AccessProfileId != Guid.Empty)
        {
            var byId = await _accessProfileRepository.GetByIdForTenantAsync(
                _currentUser.AccessProfileId,
                tenantId,
                cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        var role = _currentUser.Roles.FirstOrDefault();
        if (role is null)
        {
            return null;
        }

        var systemProfile = await _accessProfileRepository.GetSystemProfileByBaseRoleForTenantAsync(
            role,
            tenantId,
            cancellationToken);
        if (systemProfile is not null)
        {
            return systemProfile;
        }

        return role switch
        {
            ApplicationRoles.Admin => AccessProfile.CreateSystem(ApplicationRoles.Admin, ApplicationRoles.Admin, Permissions.AdminDefaults()).Value,
            ApplicationRoles.Veterinarian => AccessProfile.CreateSystem(ApplicationRoles.Veterinarian, ApplicationRoles.Veterinarian, Permissions.VeterinarianDefaults()).Value,
            ApplicationRoles.Receptionist => AccessProfile.CreateSystem(ApplicationRoles.Receptionist, ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults()).Value,
            ApplicationRoles.Cashier => AccessProfile.CreateSystem(ApplicationRoles.Cashier, ApplicationRoles.Cashier, Permissions.CashierDefaults()).Value,
            _ => null
        };
    }
}
