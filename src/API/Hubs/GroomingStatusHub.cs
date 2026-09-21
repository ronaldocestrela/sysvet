using Core.Application.Common.Interfaces;
using Core.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

/// <summary>
/// Realtime grooming board updates scoped by tenant.
/// </summary>
[Authorize]
public sealed class GroomingStatusHub : Hub
{
    public const string HubPath = "/hubs/grooming-status";

    /// <summary>
    /// SignalR group name for a tenant's grooming subscribers.
    /// </summary>
    public static string TenantGroup(Guid tenantId) => $"tenant-{tenantId}";

    private readonly IPermissionChecker _permissionChecker;
    private readonly ICurrentUser _currentUser;

    public GroomingStatusHub(IPermissionChecker permissionChecker, ICurrentUser currentUser)
    {
        _permissionChecker = permissionChecker;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.TenantId == Guid.Empty)
        {
            Context.Abort();
            return;
        }

        if (!await _permissionChecker.HasPermissionAsync(Permissions.GroomingRead))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(_currentUser.TenantId));
        await base.OnConnectedAsync();
    }
}
