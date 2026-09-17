using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Online adapter: forwards tutor operations to the REST API via <see cref="ApiClient"/>.
/// </summary>
public sealed class HttpTutorStore : ITutorStore
{
    private readonly ApiClient _apiClient;

    /// <summary>Creates the store.</summary>
    public HttpTutorStore(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc />
    public Task<Result<PagedResultDto<TutorDto>>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<PagedResultDto<TutorDto>>($"/api/v1/tutors?page={page}&pageSize={pageSize}", cancellationToken);

    /// <inheritdoc />
    public Task<Result<TutorDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<TutorDto>($"/api/v1/tutors/{id}", cancellationToken);

    /// <inheritdoc />
    public Task<Result<Guid>> CreateAsync(CreateTutorRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<CreateTutorRequest, Guid>("/api/v1/tutors", request, request.Id, cancellationToken);

    /// <inheritdoc />
    public Task<Result> UpdateAsync(UpdateTutorRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PutAsync($"/api/v1/tutors/{request.Id}", request, Guid.NewGuid(), cancellationToken);

    /// <inheritdoc />
    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync($"/api/v1/tutors/{id}", Guid.NewGuid(), cancellationToken);
}
