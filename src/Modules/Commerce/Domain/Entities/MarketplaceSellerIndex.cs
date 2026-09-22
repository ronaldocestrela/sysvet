using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// Global map Mercado Livre seller user id to tenant for anonymous webhooks.
/// </summary>
public sealed class MarketplaceSellerIndex : Entity
{
    public long MercadoLivreUserId { get; private set; }
    public Guid TenantId { get; private set; }

#pragma warning disable CS8618
    private MarketplaceSellerIndex() { }
#pragma warning restore CS8618

    /// <summary>Creates seller mapping row.</summary>
    public static MarketplaceSellerIndex Create(long mercadoLivreUserId, Guid tenantId) =>
        new()
        {
            Id = Guid.NewGuid(),
            MercadoLivreUserId = mercadoLivreUserId,
            TenantId = tenantId
        };

    /// <summary>Rebinds tenant for an existing seller id.</summary>
    public void SetTenantId(Guid tenantId) => TenantId = tenantId;
}
