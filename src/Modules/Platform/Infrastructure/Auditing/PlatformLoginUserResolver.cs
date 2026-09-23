using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Auditing;

/// <summary>Resolves tenant id from Identity users for login audit (9.7).</summary>
public sealed class PlatformLoginUserResolver : IPlatformLoginUserResolver
{
    private readonly UserManager<AppUser> _userManager;

    /// <summary>Creates the resolver.</summary>
    public PlatformLoginUserResolver(UserManager<AppUser> userManager) => _userManager = userManager;

    /// <inheritdoc />
    public async Task<Guid?> ResolveTenantIdAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || user.TenantId == Guid.Empty)
        {
            return null;
        }

        return user.TenantId;
    }
}
