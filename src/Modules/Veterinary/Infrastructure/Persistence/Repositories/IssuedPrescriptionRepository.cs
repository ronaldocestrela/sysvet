using Core.Domain;
using Microsoft.EntityFrameworkCore;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class IssuedPrescriptionRepository : IIssuedPrescriptionRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public IssuedPrescriptionRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(IssuedPrescription prescription, CancellationToken cancellationToken = default) =>
        await _dbContext.IssuedPrescriptions.AddAsync(prescription, cancellationToken);

    public async Task<IssuedPrescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.IssuedPrescriptions.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<IssuedPrescription>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var prescriptions = await _dbContext.IssuedPrescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);
        return prescriptions.OrderByDescending(p => p.UpdatedAt).ToList();
    }

    public async Task<Result> ReplaceDraftItemsAsync(
        Guid prescriptionId,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items,
        CancellationToken cancellationToken = default)
    {
        var prescription = await GetByIdAsync(prescriptionId, cancellationToken);
        if (prescription is null)
        {
            return Result.Failure(VetErrors.IssuedPrescription.NotFound);
        }

        await _dbContext.PrescriptionItems
            .Where(i => i.IssuedPrescriptionId == prescriptionId)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var entry in _dbContext.ChangeTracker.Entries<PrescriptionItem>()
                     .Where(e => e.Entity.IssuedPrescriptionId == prescriptionId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }

        var replace = prescription.ReplaceDraftItems(items);
        if (replace.IsFailure)
        {
            return replace;
        }

        foreach (var item in prescription.Items)
        {
            await _dbContext.PrescriptionItems.AddAsync(item, cancellationToken);
        }

        return Result.Success();
    }

    public void Update(IssuedPrescription prescription) => _dbContext.IssuedPrescriptions.Update(prescription);
}
