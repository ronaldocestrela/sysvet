using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Client CRM port for pets — same DTO contract as <c>/api/v1/pets</c>.
/// </summary>
public interface IPetStore
{
    /// <summary>Lists pets with paging.</summary>
    Task<Result<PagedResultDto<PetDto>>> ListAsync(int page, int pageSize, Guid? tutorId = null, CancellationToken cancellationToken = default);

    /// <summary>Loads a pet by id.</summary>
    Task<Result<PetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a pet linked to a local tutor.</summary>
    Task<Result<Guid>> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates mutable pet fields.</summary>
    Task<Result> UpdateAsync(UpdatePetRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a pet.</summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
