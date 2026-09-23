using Platform.Application.Abstractions;

namespace Platform.Infrastructure.ApiKeys;

/// <summary>Scoped partner authentication state for the current request (9.7).</summary>
public sealed class PartnerRequestContext : IPartnerRequestContext
{
    /// <inheritdoc />
    public Guid? AuthenticatedTenantId { get; set; }
}
