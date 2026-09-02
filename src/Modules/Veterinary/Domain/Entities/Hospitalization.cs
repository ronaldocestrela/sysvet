using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Veterinary.Domain.Entities;

public enum HospitalizationStatus
{
    Admitted = 1,
    Discharged = 2
}

public class Hospitalization : AggregateRoot
{
    public Guid PetId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public string Reason { get; private set; }
    public DateTimeOffset AdmittedAt { get; private set; }
    public DateTimeOffset? DischargedAt { get; private set; }
    public HospitalizationStatus Status { get; private set; }

    private readonly List<PrescriptionExecution> _prescriptionExecutions = new();
    public IReadOnlyCollection<PrescriptionExecution> PrescriptionExecutions => _prescriptionExecutions.AsReadOnly();

    private Hospitalization() 
    { 
        Reason = string.Empty;
    } // EF Core

    private Hospitalization(Guid id, Guid petId, Guid veterinarianId, string reason)
    {
        Id = id;
        PetId = petId;
        VeterinarianId = veterinarianId;
        Reason = reason;
        AdmittedAt = DateTimeOffset.UtcNow;
        Status = HospitalizationStatus.Admitted;
    }

    public static Result<Hospitalization> Admit(Guid id, Guid petId, Guid veterinarianId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<Hospitalization>(new Error("Hospitalization.InvalidReason", "Reason for admission cannot be empty."));
        }

        return Result<Hospitalization>.Success(new Hospitalization(id, petId, veterinarianId, reason));
    }

    public Result<bool> Discharge()
    {
        if (Status == HospitalizationStatus.Discharged)
        {
            return Result.Failure<bool>(new Error("Hospitalization.AlreadyDischarged", "The hospitalization is already discharged."));
        }

        Status = HospitalizationStatus.Discharged;
        DischargedAt = DateTimeOffset.UtcNow;
        return Result<bool>.Success(true);
    }

    public Result<bool> ExecutePrescription(string medicationName, string dose, string notes, Guid executedBy)
    {
        if (Status == HospitalizationStatus.Discharged)
        {
            return Result.Failure<bool>(new Error("Hospitalization.Discharged", "Cannot execute prescriptions for a discharged patient."));
        }

        var execution = PrescriptionExecution.Create(Guid.NewGuid(), Id, medicationName, dose, notes, executedBy).Value;
        _prescriptionExecutions.Add(execution);

        return Result<bool>.Success(true);
    }
}
