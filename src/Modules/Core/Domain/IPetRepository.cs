using Core.Domain.Entities;

namespace Core.Domain;

public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default);
}
