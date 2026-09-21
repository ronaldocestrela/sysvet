using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Configurations;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Inventory.Domain.Entities;
using Sales.Domain.Entities;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;
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

    /// <summary>Local clinical quotes (Fase 4.5).</summary>
    public DbSet<ClinicalQuote> ClinicalQuotes => Set<ClinicalQuote>();

    /// <summary>Local clinical quote lines.</summary>
    public DbSet<ClinicalQuoteItem> ClinicalQuoteItems => Set<ClinicalQuoteItem>();

    /// <summary>Local ward units (Fase 4.6).</summary>
    public DbSet<WardUnit> WardUnits => Set<WardUnit>();

    /// <summary>Local beds.</summary>
    public DbSet<Bed> Beds => Set<Bed>();

    /// <summary>Local hospitalizations.</summary>
    public DbSet<Hospitalization> Hospitalizations => Set<Hospitalization>();

    /// <summary>Local medication orders.</summary>
    public DbSet<HospitalMedicationOrder> HospitalMedicationOrders => Set<HospitalMedicationOrder>();

    /// <summary>Local administration slots.</summary>
    public DbSet<MedicationAdministration> MedicationAdministrations => Set<MedicationAdministration>();

    /// <summary>Local inpatient progress notes.</summary>
    public DbSet<HospitalizationProgressNote> HospitalizationProgressNotes => Set<HospitalizationProgressNote>();

    /// <summary>Local inpatient procedures.</summary>
    public DbSet<HospitalProcedure> HospitalProcedures => Set<HospitalProcedure>();

    /// <summary>Local inventory products (Fase 5.1).</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Local product lots.</summary>
    public DbSet<ProductLot> ProductLots => Set<ProductLot>();

    /// <summary>Local product balance projection.</summary>
    public DbSet<ProductBalance> ProductBalances => Set<ProductBalance>();

    /// <summary>Local suppliers.</summary>
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>Local stock movement ledger.</summary>
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    /// <summary>Local POS orders (Fase 6.2).</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Local order lines.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>Local order payments.</summary>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>Local cash register sessions.</summary>
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();

    /// <summary>Local commission rules (sync pull).</summary>
    public DbSet<CommissionRule> SalesCommissionRules => Set<CommissionRule>();

    /// <summary>Local product kits (sync pull).</summary>
    public DbSet<ProductKit> ProductKits => Set<ProductKit>();

    /// <summary>Local kit components.</summary>
    public DbSet<KitComponent> KitComponents => Set<KitComponent>();

    /// <summary>Local service packages (sync pull).</summary>
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();

    /// <summary>Local prepaid balances (sync pull + optimistic pay/consume).</summary>
    public DbSet<PrepaidBalance> PrepaidBalances => Set<PrepaidBalance>();

    /// <summary>Local grooming appointments (sync pull + outbox).</summary>
    public DbSet<GroomingAppointment> GroomingAppointments => Set<GroomingAppointment>();

    /// <summary>Local groomer schedule slots.</summary>
    public DbSet<GroomingSlot> GroomingSlots => Set<GroomingSlot>();

    /// <summary>Local grooming digital records.</summary>
    public DbSet<GroomingRecord> GroomingRecords => Set<GroomingRecord>();

    /// <summary>Local grooming service catalog.</summary>
    public DbSet<GroomingService> GroomingServices => Set<GroomingService>();

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
        modelBuilder.ApplyConfiguration(new OfflineClinicalQuoteConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineClinicalQuoteItemConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineWardUnitConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineBedConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineHospitalizationConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineHospitalMedicationOrderConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineMedicationAdministrationConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineHospitalizationProgressNoteConfiguration());
        modelBuilder.ApplyConfiguration(new OfflineHospitalProcedureConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineInventoryProductConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineProductLotConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineProductBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSupplierConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineStockMovementConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesOrderConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesOrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesPaymentRefundConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesCashRegisterConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesCommissionAccrualConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesSaleReturnConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesSaleReturnLineConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineSalesCommissionRuleConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineProductKitConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineKitComponentConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineServicePackageConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflinePrepaidBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineGroomingAppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineGroomingSlotConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineGroomingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OfflineGroomingServiceConfiguration());

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
        else if (entry.Entity is GroomingAppointment groomingAppointment)
        {
            EnqueueGroomingAppointmentOutbox(entry, groomingAppointment, outboxMessages);
        }
    }

    private static void EnqueueGroomingAppointmentOutbox(
        EntityEntry entry,
        GroomingAppointment appointment,
        ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "ScheduleGroomingAppointmentCommand",
                Payload = OutboxPayloadFactory.ScheduleGroomingAppointment(
                    appointment.Id,
                    appointment.TutorId,
                    appointment.PetId,
                    appointment.GroomerId,
                    appointment.GroomingServiceId,
                    appointment.Date,
                    appointment.DurationInMinutes,
                    appointment.Notes,
                    outboxId)
            });
            return;
        }

        if (entry.State != EntityState.Modified)
        {
            return;
        }

        var statusProperty = entry.Property(nameof(GroomingAppointment.Status));
        if (!statusProperty.IsModified)
        {
            return;
        }

        var (type, payload) = appointment.Status switch
        {
            GroomingAppointmentStatus.Confirmed => (
                "ConfirmGroomingAppointmentCommand",
                OutboxPayloadFactory.ConfirmGroomingAppointment(appointment.Id, outboxId)),
            GroomingAppointmentStatus.InProgress => (
                "StartGroomingAppointmentCommand",
                OutboxPayloadFactory.StartGroomingAppointment(appointment.Id, outboxId)),
            GroomingAppointmentStatus.Completed => (
                "CompleteGroomingAppointmentCommand",
                OutboxPayloadFactory.CompleteGroomingAppointment(appointment.Id, outboxId)),
            GroomingAppointmentStatus.Cancelled => (
                "CancelGroomingAppointmentCommand",
                OutboxPayloadFactory.CancelGroomingAppointment(appointment.Id, outboxId)),
            GroomingAppointmentStatus.NoShow => (
                "MarkNoShowGroomingAppointmentCommand",
                OutboxPayloadFactory.MarkNoShowGroomingAppointment(appointment.Id, outboxId)),
            _ => (null, null)
        };

        if (type is null || payload is null)
        {
            return;
        }

        outboxMessages.Add(new OutboxMessage { Id = outboxId, Type = type, Payload = payload });
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
