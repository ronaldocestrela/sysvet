using Core.Domain;

namespace Clients.Infrastructure.Fiscal;

/// <summary>Offline fiscal cache and document reads (7.6).</summary>
public interface IFiscalStore
{
    Task<Result<bool>> RefreshPosBundleAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfflineFiscalDocument>> GetDocumentsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<OfflineFiscalDocument?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> GetDeviceNfceSeriesAsync(CancellationToken cancellationToken = default);

    Task SetDeviceNfceSeriesAsync(int series, CancellationToken cancellationToken = default);
}
