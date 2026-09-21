using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Finance;

/// <summary>
/// Online API client for finance statement PDF export (Fase 7.4).
/// </summary>
public sealed class FinanceReportsApiService
{
    private readonly ApiClient _apiClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public FinanceReportsApiService(ApiClient apiClient, Sync.ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public Task<Result<DownloadedFile>> ExportPdfAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<DownloadedFile>(OfflineError()));
        }

        var url = $"/api/v1/finance/statements/export?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&format=Pdf";
        return _apiClient.DownloadGetAsync(url, cancellationToken);
    }

    private static Error OfflineError() =>
        new("Finance.Offline", "Exportação PDF requer conexão com a API.");
}
