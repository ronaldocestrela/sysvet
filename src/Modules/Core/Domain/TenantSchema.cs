namespace Core.Domain;

/// <summary>
/// Canonical SQL schema name for a tenant (ADR-003), shared by API middleware and public filters.
/// </summary>
public static class TenantSchema
{
    /// <summary>
    /// Builds the runtime schema name <c>tenant_{guid:N}</c> in lowercase invariant form.
    /// </summary>
    public static string FromId(Guid tenantId) =>
        $"tenant_{tenantId:N}".ToLowerInvariant();
}
