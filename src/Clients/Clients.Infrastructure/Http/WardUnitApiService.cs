using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>REST adapter for ward unit catalog management.</summary>
public sealed class WardUnitApiService : IWardUnitApiService
{
    private readonly ApiClient _apiClient;

    public WardUnitApiService(ApiClient apiClient) => _apiClient = apiClient;

    public Task<Result<IReadOnlyList<WardUnitListItemDto>>> ListAsync(bool activeOnly = true, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<WardUnitListItemDto>>($"/api/v1/ward-units?activeOnly={activeOnly.ToString().ToLowerInvariant()}", cancellationToken);

    public Task<Result<Guid>> CreateAsync(string name, IReadOnlyList<WardBedInputDto> beds, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<CreateWardUnitRequestDto, Guid>(
            "/api/v1/ward-units",
            new CreateWardUnitRequestDto { Name = name, Beds = beds },
            Guid.NewGuid(),
            cancellationToken);
}
