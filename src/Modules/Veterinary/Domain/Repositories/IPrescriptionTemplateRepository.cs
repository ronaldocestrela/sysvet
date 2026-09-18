using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for prescription templates.</summary>
public interface IPrescriptionTemplateRepository
{
    Task<PrescriptionTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrescriptionTemplate>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task AddAsync(PrescriptionTemplate template, CancellationToken cancellationToken = default);

    void Update(PrescriptionTemplate template);
}
