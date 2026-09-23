using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Intelligence;

/// <summary>HTTP client for tenant intelligence dashboards (roadmap 10.1).</summary>
public interface IIntelligenceApiService
{
    /// <summary>Loads the operational dashboard for the current user.</summary>
    Task<Result<TenantDashboardClientDto>> GetDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads layout for an access profile.</summary>
    Task<Result<ProfileDashboardLayoutClientDto>> GetLayoutAsync(Guid accessProfileId, CancellationToken cancellationToken = default);

    /// <summary>Persists layout for an access profile.</summary>
    Task<Result> SaveLayoutAsync(Guid accessProfileId, ProfileDashboardLayoutClientDto layout, CancellationToken cancellationToken = default);
}

/// <summary>REST implementation of intelligence dashboard APIs.</summary>
public sealed class IntelligenceApiService : IIntelligenceApiService
{
    private readonly ApiClient _apiClient;

    /// <summary>Creates the service.</summary>
    public IntelligenceApiService(ApiClient apiClient) => _apiClient = apiClient;

    /// <inheritdoc />
    public Task<Result<TenantDashboardClientDto>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<TenantDashboardClientDto>("/api/v1/intelligence/dashboard", cancellationToken);

    /// <inheritdoc />
    public Task<Result<ProfileDashboardLayoutClientDto>> GetLayoutAsync(Guid accessProfileId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<ProfileDashboardLayoutClientDto>($"/api/v1/intelligence/dashboard-layouts/{accessProfileId}", cancellationToken);

    /// <inheritdoc />
    public Task<Result> SaveLayoutAsync(Guid accessProfileId, ProfileDashboardLayoutClientDto layout, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync(
            $"/api/v1/intelligence/dashboard-layouts/{accessProfileId}",
            layout,
            idempotencyKey: null,
            cancellationToken);
}

/// <summary>Dashboard API payload.</summary>
public sealed class TenantDashboardClientDto
{
    public DateOnly BusinessDate { get; init; }
    public IReadOnlyList<DashboardWidgetClientDto> Widgets { get; init; } = Array.Empty<DashboardWidgetClientDto>();
}

/// <summary>Widget entry from dashboard API.</summary>
public sealed class DashboardWidgetClientDto
{
    public string Key { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public object? Data { get; init; }
}

/// <summary>Layout API payload.</summary>
public sealed class ProfileDashboardLayoutClientDto
{
    public Guid AccessProfileId { get; init; }
    public IReadOnlyList<DashboardWidgetSlotClientDto> Slots { get; init; } = Array.Empty<DashboardWidgetSlotClientDto>();
}

/// <summary>Layout slot.</summary>
public sealed class DashboardWidgetSlotClientDto
{
    public string WidgetKey { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Sales today KPI JSON shape.</summary>
public sealed class SalesTodayClientDto
{
    public decimal NetAmount { get; init; }
    public int OrderCount { get; init; }
    public decimal AverageTicket { get; init; }
}
