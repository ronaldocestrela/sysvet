using Commerce.Domain.Entities;

namespace Commerce.Domain.Repositories;

/// <summary>Tenant Mercado Livre settings singleton.</summary>
public interface IMercadoLivreSettingsRepository
{
    Task<MercadoLivreSettings?> GetAsync(CancellationToken cancellationToken = default);
    void Add(MercadoLivreSettings settings);
    void Update(MercadoLivreSettings settings);
}
