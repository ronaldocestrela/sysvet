namespace Core.Domain;

public interface ITenantContext
{
    Guid TenantId { get; }
    string SchemaName { get; }
}
