using Core.Domain;

namespace API.Services;

public class DefaultTenantContext : ITenantContext
{
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public string SchemaName { get; set; } = "dbo";
    public string ConnectionString { get; set; } = string.Empty;
}
