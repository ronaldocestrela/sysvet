using Core.Domain;

namespace Core.Infrastructure.Tenancy;

/// <summary>
/// Fallback tenant context used at startup and in tests when no authenticated tenant is available.
/// </summary>
public sealed class DefaultTenantContext : ITenantContext
{
    /// <inheritdoc />
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <inheritdoc />
    public Guid UserId { get; set; } = Guid.Empty;

    /// <inheritdoc />
    public string SchemaName { get; set; } = "dbo";

    /// <inheritdoc />
    public string ConnectionString { get; set; } = string.Empty;
}
