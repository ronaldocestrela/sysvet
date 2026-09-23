namespace Platform.Application.Abstractions;

/// <summary>Partner API key authentication context for the current HTTP request (9.7).</summary>
public interface IPartnerRequestContext
{
    /// <summary>Tenant bound to a validated partner API key.</summary>
    Guid? AuthenticatedTenantId { get; set; }
}
