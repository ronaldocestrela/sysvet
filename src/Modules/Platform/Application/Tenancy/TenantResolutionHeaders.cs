namespace Platform.Application.Tenancy;

/// <summary>
/// HTTP headers used for optional tenant resolution when JWT is absent.
/// </summary>
public static class TenantResolutionHeaders
{
    /// <summary>Explicit tenant GUID.</summary>
    public const string TenantId = "X-Tenant-Id";

    /// <summary>Explicit tenant slug (resolved via catalog).</summary>
    public const string TenantSlug = "X-Tenant-Slug";
}
