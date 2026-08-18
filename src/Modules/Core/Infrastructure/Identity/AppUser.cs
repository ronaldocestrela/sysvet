using Microsoft.AspNetCore.Identity;

namespace Core.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    /// <summary>
    /// ID do tenant ao qual este usuário pertence.
    /// Para o SuperAdmin, este campo pode ser especial ou nulo.
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;
}
