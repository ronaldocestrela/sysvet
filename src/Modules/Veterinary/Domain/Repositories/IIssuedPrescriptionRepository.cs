using Core.Domain;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for issued prescriptions.</summary>
public interface IIssuedPrescriptionRepository
{
    Task<IssuedPrescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IssuedPrescription>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task AddAsync(IssuedPrescription prescription, CancellationToken cancellationToken = default);

    /// <summary>Replaces draft lines after clearing persisted items (avoids EF collection tracking issues).</summary>
    Task<Result> ReplaceDraftItemsAsync(
        Guid prescriptionId,
        IEnumerable<(Guid ItemId, string MedicationName, string Concentration, string Dose, string Route, string Frequency, string Duration, string Instructions, int SortOrder)> items,
        CancellationToken cancellationToken = default);

    void Update(IssuedPrescription prescription);
}
