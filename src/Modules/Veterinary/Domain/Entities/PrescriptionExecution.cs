using Core.Domain;
using System;

namespace Veterinary.Domain.Entities;

public class PrescriptionExecution : Entity
{
    public Guid HospitalizationId { get; private set; }
    public string MedicationName { get; private set; }
    public string Dose { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; }
    public string Notes { get; private set; }
    public Guid ExecutedBy { get; private set; }

    private PrescriptionExecution() 
    { 
        MedicationName = string.Empty;
        Dose = string.Empty;
        Notes = string.Empty;
    } // EF Core

    private PrescriptionExecution(Guid id, Guid hospitalizationId, string medicationName, string dose, string notes, Guid executedBy)
    {
        Id = id;
        HospitalizationId = hospitalizationId;
        MedicationName = medicationName;
        Dose = dose;
        Notes = notes;
        ExecutedBy = executedBy;
        ExecutedAt = DateTimeOffset.UtcNow;
    }

    internal static Result<PrescriptionExecution> Create(Guid id, Guid hospitalizationId, string medicationName, string dose, string notes, Guid executedBy)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
        {
            return Result.Failure<PrescriptionExecution>(new Error("PrescriptionExecution.InvalidMedicationName", "Medication name cannot be empty."));
        }

        return Result<PrescriptionExecution>.Success(new PrescriptionExecution(id, hospitalizationId, medicationName, dose, notes, executedBy));
    }
}
