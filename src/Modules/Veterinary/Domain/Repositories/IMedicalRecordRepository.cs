using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);
    void Update(MedicalRecord medicalRecord);
}
