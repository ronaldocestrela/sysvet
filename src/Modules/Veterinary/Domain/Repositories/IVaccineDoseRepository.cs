using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IVaccineDoseRepository
{
    Task<VaccineDose?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<VaccineDose>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task AddAsync(VaccineDose vaccineDose, CancellationToken cancellationToken = default);
    void Update(VaccineDose vaccineDose);
}
