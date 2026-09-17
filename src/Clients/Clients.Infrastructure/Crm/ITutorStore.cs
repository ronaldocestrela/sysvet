using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Client CRM port for tutors — same DTO contract as <c>/api/v1/tutors</c> (online or offline adapter).
/// </summary>
public interface ITutorStore
{
    /// <summary>Lists tutors with paging (local or remote).</summary>
    Task<Result<PagedResultDto<TutorDto>>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Loads a tutor by id.</summary>
    Task<Result<TutorDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a tutor and returns its id.</summary>
    Task<Result<Guid>> CreateAsync(CreateTutorRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates mutable tutor fields.</summary>
    Task<Result> UpdateAsync(UpdateTutorRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a tutor and linked pets.</summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
