using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IPrescriptionExecutionRepository
{
    Task<List<PrescriptionExecution>> GetByHospitalizationIdAsync(Guid hospitalizationId, CancellationToken cancellationToken = default);
    Task AddAsync(PrescriptionExecution prescriptionExecution, CancellationToken cancellationToken = default);
}
