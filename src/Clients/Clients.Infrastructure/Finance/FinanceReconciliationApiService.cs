using Core.Domain;

namespace Clients.Infrastructure.Finance;

/// <summary>
/// Online-only API client for card reconciliation (Fase 7.3).
/// </summary>
public sealed class FinanceReconciliationApiService
{
    private readonly Http.ApiClient _apiClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public FinanceReconciliationApiService(Http.ApiClient apiClient, Sync.ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public Task<Result<IReadOnlyList<CardReconciliationSummaryClientDto>>> ListBatchesAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<CardReconciliationSummaryClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<CardReconciliationSummaryClientDto>>(
            "/api/v1/finance/card-reconciliations",
            cancellationToken);
    }

    public Task<Result<Guid>> ImportStatementAsync(ImportCardStatementClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<ImportCardStatementClientRequest, Guid>(
            "/api/v1/finance/card-reconciliations",
            request,
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public Task<Result<IReadOnlyList<UnmatchedCardSettlementClientDto>>> ListUnmatchedAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<UnmatchedCardSettlementClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<UnmatchedCardSettlementClientDto>>(
            "/api/v1/finance/card-settlements/unmatched",
            cancellationToken);
    }

    private static Error OfflineError() =>
        new("Finance.Offline", "Conciliação de cartões requer conexão com a API.");
}

public sealed class CardReconciliationSummaryClientDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public int MatchedCount { get; init; }
    public int UnmatchedCount { get; init; }
    public int DivergentCount { get; init; }
}

public sealed class UnmatchedCardSettlementClientDto
{
    public Guid AllocationId { get; init; }
    public string Nsu { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
}

public sealed class ImportCardStatementClientRequest
{
    public string Reference { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public List<ImportCardStatementLineClientRequest> Lines { get; set; } = new();
}

public sealed class ImportCardStatementLineClientRequest
{
    public string Nsu { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Method { get; set; }
}
