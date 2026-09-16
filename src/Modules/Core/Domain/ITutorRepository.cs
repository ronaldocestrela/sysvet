using Core.Domain.Entities;

namespace Core.Domain;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Tutor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tutors with server-side paging and optional filters.
    /// </summary>
    Task<PagedList<Tutor>> SearchAsync(
        int page,
        int pageSize,
        string? nameFilter,
        string? cpfFilter,
        CancellationToken cancellationToken = default);
}
