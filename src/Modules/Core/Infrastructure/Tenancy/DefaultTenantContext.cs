using Core.Domain;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.Tenancy;

/// <summary>
/// Fallback tenant context used at startup and in tests when no authenticated tenant is available.
/// </summary>
public sealed class DefaultTenantContext : ITenantContext
{
    /// <summary>
    /// Creates a context whose schema name follows <see cref="TenancySettings.DefaultSchema"/> (ADR-003 design-time baseline).
    /// </summary>
    public DefaultTenantContext(IOptions<TenancySettings> tenancySettings)
    {
        var settings = tenancySettings.Value;
        SchemaName = settings.DefaultSchema;
        if (settings.SingleTenantId is { } tenantId && tenantId != Guid.Empty)
        {
            TenantId = tenantId;
        }
    }

    /// <summary>
    /// Parameterless constructor for design-time EF tools and tests that set properties explicitly.
    /// </summary>
    public DefaultTenantContext()
    {
        SchemaName = "dbo";
    }

    /// <inheritdoc />
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <inheritdoc />
    public Guid UserId { get; set; } = Guid.Empty;

    /// <inheritdoc />
    public string SchemaName { get; set; }

    /// <inheritdoc />
    public string ConnectionString { get; set; } = string.Empty;
}
