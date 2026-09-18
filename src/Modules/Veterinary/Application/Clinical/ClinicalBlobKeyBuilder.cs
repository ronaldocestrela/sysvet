using Core.Domain;

namespace Veterinary.Application.Clinical;

/// <summary>Builds tenant-scoped blob keys for clinical attachments.</summary>
public static class ClinicalBlobKeyBuilder
{
    /// <summary>Creates a storage key under the tenant schema partition.</summary>
    public static string Build(ITenantContext tenantContext, Guid attachmentId)
    {
        var now = DateTimeOffset.UtcNow;
        var schema = string.IsNullOrWhiteSpace(tenantContext.SchemaName) ? "dbo" : tenantContext.SchemaName;
        return $"{schema}/clinical/{now:yyyy}/{now:MM}/{attachmentId:N}";
    }
}
