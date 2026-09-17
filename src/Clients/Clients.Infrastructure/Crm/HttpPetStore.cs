using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Online adapter: forwards pet operations to the REST API via <see cref="ApiClient"/>.
/// </summary>
public sealed class HttpPetStore : IPetStore
{
    private readonly ApiClient _apiClient;

    /// <summary>Creates the store.</summary>
    public HttpPetStore(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc />
    public Task<Result<PagedResultDto<PetDto>>> ListAsync(int page, int pageSize, Guid? tutorId = null, CancellationToken cancellationToken = default)
    {
        var url = tutorId.HasValue && tutorId.Value != Guid.Empty
            ? $"/api/v1/pets?page={page}&pageSize={pageSize}&tutorId={tutorId}"
            : $"/api/v1/pets?page={page}&pageSize={pageSize}";
        return _apiClient.GetAsync<PagedResultDto<PetDto>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<PetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<PetDto>($"/api/v1/pets/{id}", cancellationToken);

    /// <inheritdoc />
    public Task<Result<Guid>> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<CreatePetRequest, Guid>("/api/v1/pets", request, request.Id, cancellationToken);

    /// <inheritdoc />
    public Task<Result> UpdateAsync(UpdatePetRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PutAsync($"/api/v1/pets/{request.Id}", request, Guid.NewGuid(), cancellationToken);

    /// <inheritdoc />
    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync($"/api/v1/pets/{id}", Guid.NewGuid(), cancellationToken);
}
