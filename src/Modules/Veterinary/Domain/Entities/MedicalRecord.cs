using Core.Domain;
using System;

namespace Veterinary.Domain.Entities;

public enum MedicalRecordStatus
{
    Draft = 1,
    Finalized = 2
}

public class MedicalRecord : AggregateRoot
{
    public Guid AppointmentId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public string Diagnosis { get; private set; }
    public string Prescription { get; private set; }
    public MedicalRecordStatus Status { get; private set; }

    private MedicalRecord() 
    { 
        Diagnosis = string.Empty;
        Prescription = string.Empty;
    } // EF Core

    private MedicalRecord(Guid id, Guid appointmentId, Guid veterinarianId, Guid tutorId, Guid petId)
    {
        Id = id;
        AppointmentId = appointmentId;
        VeterinarianId = veterinarianId;
        TutorId = tutorId;
        PetId = petId;
        Diagnosis = string.Empty;
        Prescription = string.Empty;
        Status = MedicalRecordStatus.Draft;
    }

    public static Result<MedicalRecord> Create(Guid id, Guid appointmentId, Guid veterinarianId, Guid tutorId, Guid petId)
    {
        return Result<MedicalRecord>.Success(new MedicalRecord(id, appointmentId, veterinarianId, tutorId, petId));
    }

    public Result<bool> AppendDiagnosis(string diagnosis)
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(new Error("MedicalRecord.Finalized", "Cannot modify a finalized medical record."));
        }

        Diagnosis += string.IsNullOrEmpty(Diagnosis) ? diagnosis : "\n" + diagnosis;
        return Result<bool>.Success(true);
    }

    public Result<bool> AppendPrescription(string prescription)
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(new Error("MedicalRecord.Finalized", "Cannot modify a finalized medical record."));
        }

        Prescription += string.IsNullOrEmpty(Prescription) ? prescription : "\n" + prescription;
        return Result<bool>.Success(true);
    }

    public Result<bool> FinalizeRecord()
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(new Error("MedicalRecord.AlreadyFinalized", "The medical record is already finalized."));
        }

        Status = MedicalRecordStatus.Finalized;
        return Result<bool>.Success(true);
    }
}
