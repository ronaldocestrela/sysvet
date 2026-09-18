using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Configurations;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Clients.Infrastructure;

/// <summary>
/// Client-local SQLite database for offline CRM and the transactional outbox (ADR-002).
/// </summary>
public class OfflineDbContext : DbContext
{
    private readonly ISqliteFilePersistence _filePersistence;

    /// <summary>
    /// When true, <see cref="SaveChangesAsync"/> skips outbox enqueue (remote pull apply).
    /// </summary>
    public bool SuppressOutbox { get; set; }

    /// <summary>
    /// Creates a context bound to DI options and host-specific file persistence.
    /// </summary>
    public OfflineDbContext(DbContextOptions<OfflineDbContext> options, ISqliteFilePersistence filePersistence)
        : base(options)
    {
        _filePersistence = filePersistence;
    }

    /// <summary>Tutors stored locally for offline CRM.</summary>
    public DbSet<Tutor> Tutors => Set<Tutor>();

    /// <summary>Pets stored locally for offline CRM.</summary>
    public DbSet<Pet> Pets => Set<Pet>();

    /// <summary>Pending sync commands (client-only).</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Pull cursor singleton.</summary>
    public DbSet<SyncState> SyncState => Set<SyncState>();

    /// <summary>Local clinical appointments (Fase 4.1).</summary>
    public DbSet<Appointment> Appointments => Set<Appointment>();

    /// <summary>Local schedule slots mirror.</summary>
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();

    /// <summary>Local medical records (Fase 4.2).</summary>
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    /// <summary>Local evolution notes.</summary>
    public DbSet<EvolutionNote> EvolutionNotes => Set<EvolutionNote>();

    /// <summary>Local clinical exams (Fase 4.3).</summary>
    public DbSet<ClinicalExam> ClinicalExams => Set<ClinicalExam>();

    /// <summary>Local issued prescriptions metadata.</summary>
    public DbSet<IssuedPrescription> IssuedPrescriptions => Set<IssuedPrescription>();

    /// <summary>Local prescription lines.</summary>
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    /// <summary>Local attachment metadata (bytes online only).</summary>
    public DbSet<ClinicalAttachment> ClinicalAttachments => Set<ClinicalAttachment>();

    /// <summary>Local prescription templates (read-only mirror).</summary>
    public DbSet<PrescriptionTemplate> PrescriptionTemplates => Set<PrescriptionTemplate>();

    /// <summary>Local template lines.</summary>
    public DbSet<PrescriptionTemplateItem> PrescriptionTemplateItems => Set<PrescriptionTemplateItem>();

    /// <summary>Local vaccine protocols (Fase 4.4).</summary>
    public DbSet<VaccineProtocol> VaccineProtocols => Set<VaccineProtocol>();

    /// <summary>Local vaccine protocol dose lines.</summary>
    public DbSet<VaccineProtocolDose> VaccineProtocolDoses => Set<VaccineProtocolDose>();

    /// <summary>Local applied vaccine doses.</summary>
    public DbSet<VaccineDose> VaccineDoses => Set<VaccineDose>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OfflineTutorConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePetConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new SyncStateConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineAppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineScheduleSlotConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineMedicalRecordConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineEvolutionNoteConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineClinicalExamConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineIssuedPrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePrescriptionItemConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineClinicalAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePrescriptionTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePrescriptionTemplateItemConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineVaccineProtocolConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineVaccineProtocolDoseConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineVaccineDoseConfiguration());

        modelBuilder.Entity<Tutor>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var outboxMessages = new List<OutboxMessage>();

        if (!SuppressOutbox)
        {
            var entries = ChangeTracker.Entries<Entity>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                EnqueueOutbox(entry, outboxMessages);
            }
        }

        if (outboxMessages.Count > 0)
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        var count = await base.SaveChangesAsync(cancellationToken);

        if (count > 0)
        {
            await SqliteFileHelper.FlushToPersistentStorageAsync(this, _filePersistence, cancellationToken);
        }

        return count;
    }

    private static void EnqueueOutbox(EntityEntry entry, ICollection<OutboxMessage> outboxMessages)
    {
        if (entry.Entity is Tutor tutor)
        {
            EnqueueTutorOutbox(entry, tutor, outboxMessages);
        }
        else if (entry.Entity is Pet pet)
        {
            EnqueuePetOutbox(entry, pet, outboxMessages);
        }
        else if (entry.Entity is Appointment appointment)
        {
            EnqueueAppointmentOutbox(entry, appointment, outboxMessages);
        }
        else if (entry.Entity is MedicalRecord medicalRecord)
        {
            EnqueueMedicalRecordOutbox(entry, medicalRecord, outboxMessages);
        }
        else if (entry.Entity is EvolutionNote evolutionNote)
        {
            EnqueueEvolutionNoteOutbox(entry, evolutionNote, outboxMessages);
        }
        else if (entry.Entity is ClinicalExam clinicalExam)
        {
            EnqueueClinicalExamOutbox(entry, clinicalExam, outboxMessages);
        }
        else if (entry.Entity is VaccineDose vaccineDose)
        {
            EnqueueVaccineDoseOutbox(entry, vaccineDose, outboxMessages);
        }
    }

    private static void EnqueueVaccineDoseOutbox(EntityEntry entry, VaccineDose dose, ICollection<OutboxMessage> outboxMessages)
    {
        if (entry.State != EntityState.Added)
        {
            return;
        }

        var outboxId = Guid.NewGuid();
        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "RegisterVaccineDoseCommand",
            Payload = OutboxPayloadFactory.RegisterVaccineDose(
                dose.Id,
                dose.PetId,
                dose.Name,
                dose.BatchNumber,
                dose.AppliedAt,
                dose.NextDueDate,
                dose.ProtocolDoseId,
                outboxId)
        });
    }

    private static void EnqueueClinicalExamOutbox(EntityEntry entry, ClinicalExam exam, ICollection<OutboxMessage> outboxMessages)
    {
        if (entry.State != EntityState.Added)
        {
            return;
        }

        var outboxId = Guid.NewGuid();
        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "RequestClinicalExamCommand",
            Payload = OutboxPayloadFactory.RequestClinicalExam(exam.AppointmentId, exam.Name, exam.Category.ToString(), outboxId)
        });
    }

    private static void EnqueueAppointmentOutbox(EntityEntry entry, Appointment appointment, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "ScheduleAppointmentCommand",
                Payload = OutboxPayloadFactory.ScheduleAppointment(
                    appointment.Id,
                    appointment.TutorId,
                    appointment.PetId,
                    appointment.VeterinarianId,
                    appointment.Date,
                    appointment.DurationInMinutes,
                    appointment.Reason,
                    outboxId)
            });
            return;
        }

        if (entry.State != EntityState.Modified)
        {
            return;
        }

        var statusProperty = entry.Property(nameof(Appointment.Status));
        if (!statusProperty.IsModified)
        {
            return;
        }

        var (type, payload) = appointment.Status switch
        {
            AppointmentStatus.Confirmed => (
                "ConfirmAppointmentCommand",
                OutboxPayloadFactory.ConfirmAppointment(appointment.Id, outboxId)),
            AppointmentStatus.InProgress => (
                "StartAppointmentCommand",
                OutboxPayloadFactory.StartAppointment(appointment.Id, outboxId)),
            AppointmentStatus.Cancelled => (
                "CancelAppointmentCommand",
                OutboxPayloadFactory.CancelAppointment(appointment.Id, outboxId)),
            AppointmentStatus.NoShow => (
                "MarkNoShowAppointmentCommand",
                OutboxPayloadFactory.MarkNoShowAppointment(appointment.Id, outboxId)),
            _ => (null, null)
        };

        if (type is null || payload is null)
        {
            return;
        }

        outboxMessages.Add(new OutboxMessage { Id = outboxId, Type = type, Payload = payload });
    }

    private static void EnqueueMedicalRecordOutbox(EntityEntry entry, MedicalRecord record, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();
        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "CreateMedicalRecordCommand",
                Payload = OutboxPayloadFactory.CreateMedicalRecord(record.AppointmentId, outboxId)
            });
            return;
        }

        if (entry.State != EntityState.Modified)
        {
            return;
        }

        if (PropertyModified(entry, nameof(MedicalRecord.Status)) &&
            record.Status == MedicalRecordStatus.Finalized)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "FinalizeMedicalRecordCommand",
                Payload = OutboxPayloadFactory.FinalizeMedicalRecord(record.Id, outboxId)
            });
            return;
        }

        if (PropertyModified(entry, nameof(MedicalRecord.Anamnesis)))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "UpdateAnamnesisCommand",
                Payload = OutboxPayloadFactory.UpdateAnamnesis(record.Id, record.Anamnesis, outboxId)
            });
        }

        if (PropertyModified(entry, nameof(MedicalRecord.Diagnosis)))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "SetDiagnosisCommand",
                Payload = OutboxPayloadFactory.SetDiagnosis(record.Id, record.Diagnosis, Guid.NewGuid())
            });
        }

        if (PropertyModified(entry, nameof(MedicalRecord.Prescription)))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "SetConductCommand",
                Payload = OutboxPayloadFactory.SetConduct(record.Id, record.Prescription, Guid.NewGuid())
            });
        }

        if (record.VitalSigns is not null &&
            entry.Properties.Any(p => p.IsModified && p.Metadata.Name.StartsWith("Vital", StringComparison.Ordinal)))
        {
            var vitals = record.VitalSigns;
            outboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "RecordVitalSignsCommand",
                Payload = OutboxPayloadFactory.RecordVitalSigns(
                    record.Id,
                    vitals.WeightKg,
                    vitals.TemperatureC,
                    vitals.HeartRateBpm,
                    vitals.RespiratoryRateBpm,
                    vitals.MucousMembranes,
                    vitals.CapillaryRefillTime,
                    vitals.MeasuredAt,
                    Guid.NewGuid())
            });
        }
    }

    private static void EnqueueEvolutionNoteOutbox(EntityEntry entry, EvolutionNote note, ICollection<OutboxMessage> outboxMessages)
    {
        if (entry.State != EntityState.Added)
        {
            return;
        }

        var outboxId = Guid.NewGuid();
        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "AddEvolutionNoteCommand",
            Payload = OutboxPayloadFactory.AddEvolutionNote(note.MedicalRecordId, note.Text, note.Id, note.RecordedAt, outboxId)
        });
    }

    private static bool PropertyModified(EntityEntry entry, string propertyName) =>
        entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName) is { IsModified: true };

    private static void EnqueueTutorOutbox(EntityEntry entry, Tutor tutor, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "CreateTutorCommand",
                Payload = OutboxPayloadFactory.CreateTutor(
                    tutor.Id,
                    tutor.Name,
                    tutor.Email.Address,
                    tutor.Cpf.Number,
                    tutor.Phone.Number,
                    outboxId)
            });
            return;
        }

        if (BecameDeleted(entry))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "DeleteTutorCommand",
                Payload = OutboxPayloadFactory.DeleteTutor(tutor.Id, outboxId)
            });
            return;
        }

        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "UpdateTutorCommand",
            Payload = OutboxPayloadFactory.UpdateTutor(
                tutor.Id,
                tutor.Name,
                tutor.Email.Address,
                tutor.Phone.Number,
                tutor.UpdatedAt,
                outboxId)
        });
    }

    private static void EnqueuePetOutbox(EntityEntry entry, Pet pet, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "CreatePetCommand",
                Payload = OutboxPayloadFactory.CreatePet(
                    pet.Id,
                    pet.Name,
                    pet.Species.ToString(),
                    pet.Breed,
                    pet.Sex.ToString(),
                    pet.TutorId,
                    pet.BirthDate,
                    outboxId)
            });
            return;
        }

        if (BecameDeleted(entry))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "DeletePetCommand",
                Payload = OutboxPayloadFactory.DeletePet(pet.Id, outboxId)
            });
            return;
        }

        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "UpdatePetCommand",
            Payload = OutboxPayloadFactory.UpdatePet(
                pet.Id,
                pet.Name,
                pet.Species.ToString(),
                pet.Breed,
                pet.Sex.ToString(),
                pet.BirthDate,
                pet.UpdatedAt,
                outboxId)
        });
    }

    private static bool BecameDeleted(EntityEntry entry)
    {
        if (entry.State != EntityState.Modified)
        {
            return false;
        }

        var deletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ISoftDeletable.IsDeleted));
        return deletedProperty is { IsModified: true } && deletedProperty.CurrentValue is true;
    }
}
