using Core.Domain.Entities;

namespace Core.Domain;

public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches pets with server-side paging and optional filters.
    /// </summary>
    Task<PagedList<Pet>> SearchAsync(
        int page,
        int pageSize,
        Guid? tutorId,
        string? nameFilter,
        CancellationToken cancellationToken = default);
}
