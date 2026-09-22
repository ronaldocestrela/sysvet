using Core.Domain;

namespace Platform.Domain;

/// <summary>
/// Platform module error codes for Result failures.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Tenant catalog validation errors.</summary>
    public static class Tenant
    {
        public static readonly Error NotFound = new("Platform.Tenant.NotFound", "Tenant não encontrado.");
        public static readonly Error InvalidSlug = new("Platform.Tenant.InvalidSlug", "Slug do tenant inválido.");
        public static readonly Error DuplicateSlug = new("Platform.Tenant.DuplicateSlug", "Slug do tenant já está em uso.");
        public static readonly Error InvalidId = new("Platform.Tenant.InvalidId", "Tenant id inválido.");
    }

    /// <summary>Request tenancy resolution errors.</summary>
    public static class Tenancy
    {
        public static readonly Error TenantRequired = new("Platform.Tenancy.TenantRequired", "Tenant não identificado na requisição.");
        public static readonly Error TenantMismatch = new("Platform.Tenancy.TenantMismatch", "Tenant da requisição não corresponde ao token.");
    }
}
