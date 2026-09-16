using Microsoft.AspNetCore.Identity;

namespace Core.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user with tenant and access profile linkage.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// ID do tenant ao qual este usuário pertence.
    /// Para o SuperAdmin, este campo pode ser especial ou nulo.
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Assigned fine-grained access profile within the tenant.
    /// </summary>
    public Guid AccessProfileId { get; set; } = Guid.Empty;

    /// <summary>
    /// Optional display name shown in admin UI.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// When true, the user cannot sign in.
    /// </summary>
    public bool IsDisabled { get; set; }
}
