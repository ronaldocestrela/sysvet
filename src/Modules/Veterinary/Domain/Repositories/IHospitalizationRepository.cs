using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IHospitalizationRepository
{
    Task<Hospitalization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Hospitalization hospitalization, CancellationToken cancellationToken = default);
    void Update(Hospitalization hospitalization);
}
