using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Fiscal;

/// <summary>Online API client for fiscal planning reports (Fase 7.7).</summary>
public sealed class FiscalPlanningApiService
{
    private readonly ApiClient _apiClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public FiscalPlanningApiService(ApiClient apiClient, Sync.ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    /// <summary>Loads period apuration and regime simulation from the API.</summary>
    public Task<Result<FiscalPlanningReportClientDto>> GetPlanningAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<FiscalPlanningReportClientDto>(OfflineError()));
        }

        var url = $"/api/v1/fiscal/planning?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return _apiClient.GetAsync<FiscalPlanningReportClientDto>(url, cancellationToken);
    }

    /// <summary>Exports planning report as CSV (online only).</summary>
    public Task<Result<DownloadedFile>> ExportCsvAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<DownloadedFile>(OfflineError()));
        }

        var url = $"/api/v1/fiscal/planning/export?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&format=csv";
        return _apiClient.DownloadGetAsync(url, cancellationToken);
    }

    /// <summary>Exports planning report as PDF (online only).</summary>
    public Task<Result<DownloadedFile>> ExportPdfAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<DownloadedFile>(OfflineError()));
        }

        var url = $"/api/v1/fiscal/planning/export?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&format=pdf";
        return _apiClient.DownloadGetAsync(url, cancellationToken);
    }

    private static Error OfflineError() =>
        new("Fiscal.Planning.Offline", "Planejamento fiscal requer conexão com a API.");
}

/// <summary>Client mirror of fiscal planning API response.</summary>
public sealed class FiscalPlanningReportClientDto
{
    public FiscalPeriodReportClientDto Period { get; init; } = new();
    public RegimeSimulationClientDto Simulation { get; init; } = new();
}

/// <summary>Period section for client UI.</summary>
public sealed class FiscalPeriodReportClientDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public decimal GoodsRevenue { get; init; }
    public decimal ServicesRevenue { get; init; }
    public decimal TotalGrossRevenue { get; init; }
    public decimal EstimatedIss { get; init; }
    public int AuthorizedDocumentCount { get; init; }
    public int CancelledDocumentCount { get; init; }
    public IReadOnlyList<CfopBreakdownLineClientDto> CfopBreakdown { get; init; } = Array.Empty<CfopBreakdownLineClientDto>();
}

/// <summary>CFOP line for client UI.</summary>
public sealed record CfopBreakdownLineClientDto(string Cfop, decimal GrossAmount, int DocumentCount);

/// <summary>Regime simulation section for client UI.</summary>
public sealed class RegimeSimulationClientDto
{
    public decimal SimplesEstimatedTax { get; init; }
    public decimal PresumidoEstimatedTax { get; init; }
    public decimal SimplesEffectiveRate { get; init; }
    public decimal PresumidoEffectiveRate { get; init; }
    public decimal Rbt12Goods { get; init; }
    public decimal Rbt12Services { get; init; }
    public string SuggestedRegime { get; init; } = string.Empty;
    public string Disclaimer { get; init; } = string.Empty;
    public decimal IssRatePercent { get; init; }
    public int TaxRegimeCode { get; init; }
}
