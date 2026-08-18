namespace Core.Domain;

public interface ITenantContext
{
    Guid TenantId { get; set; }
    string SchemaName { get; set; }
}
