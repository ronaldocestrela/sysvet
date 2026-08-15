using Core.Domain.Entities;

namespace Core.Domain;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<Tutor?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Tutor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
