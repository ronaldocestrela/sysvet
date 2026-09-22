using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Tenancy;

/// <summary>
/// Multi-tenant defaults applied when a request does not specify a tenant schema (see ADR-003).
/// </summary>
public class TenancySettings
{
    /// <summary>
    /// Configuration section name for tenancy settings.
    /// </summary>
    public const string SectionName = "TenancySettings";

    /// <summary>
    /// Fallback SQL schema name for tenant isolation.
    /// </summary>
    [Required]
    public string DefaultSchema { get; set; } = "dbo";

    /// <summary>
    /// When set, used as <see cref="ITenantContext.TenantId"/> for background workers and design-time scopes (single-tenant dev until Platform 9.x).
    /// </summary>
    public Guid? SingleTenantId { get; set; }
}
