using Core.Domain;
using System;

namespace Veterinary.Domain.Entities;

public class VaccineDose : AggregateRoot
{
    public Guid PetId { get; private set; }
    public string Name { get; private set; }
    public string BatchNumber { get; private set; }
    public DateTimeOffset AppliedAt { get; private set; }
    public DateTimeOffset? NextDueDate { get; private set; }

    private VaccineDose() 
    { 
        Name = string.Empty;
        BatchNumber = string.Empty;
    } // EF Core

    private VaccineDose(Guid id, Guid petId, string name, string batchNumber, DateTimeOffset appliedAt, DateTimeOffset? nextDueDate)
    {
        Id = id;
        PetId = petId;
        Name = name;
        BatchNumber = batchNumber;
        AppliedAt = appliedAt;
        NextDueDate = nextDueDate;
    }

    public static Result<VaccineDose> Create(Guid id, Guid petId, string name, string batchNumber, DateTimeOffset appliedAt, DateTimeOffset? nextDueDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<VaccineDose>(new Error("VaccineDose.InvalidName", "Vaccine name cannot be empty."));
        }

        if (appliedAt > DateTimeOffset.UtcNow)
        {
            return Result.Failure<VaccineDose>(new Error("VaccineDose.FutureApplicationDate", "Application date cannot be in the future."));
        }

        return Result<VaccineDose>.Success(new VaccineDose(id, petId, name, batchNumber, appliedAt, nextDueDate));
    }
}
