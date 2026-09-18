using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.ValueObjects;

namespace Clients.Infrastructure.Sync;

/// <summary>
/// Applies remote pull DTOs to local SQLite without enqueueing outbox messages.
/// </summary>
public sealed class OfflineSyncPullApplier
{
    private readonly OfflineDbContext _dbContext;

    /// <summary>Creates the applier.</summary>
    public OfflineSyncPullApplier(OfflineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Upserts tutors and pets from a pull page and advances the sync cursor.
    /// </summary>
    public async Task ApplyAsync(ClientPullChangesResult page, CancellationToken cancellationToken = default)
    {
        _dbContext.SuppressOutbox = true;
        try
        {
            foreach (var dto in page.Tutors)
            {
                await UpsertTutorAsync(dto, cancellationToken);
            }

            foreach (var dto in page.Pets)
            {
                await UpsertPetAsync(dto, cancellationToken);
            }

            foreach (var dto in page.Appointments)
            {
                await UpsertAppointmentAsync(dto, cancellationToken);
            }

            foreach (var dto in page.ScheduleSlots)
            {
                await UpsertScheduleSlotAsync(dto, cancellationToken);
            }

            foreach (var dto in page.MedicalRecords)
            {
                await UpsertMedicalRecordAsync(dto, cancellationToken);
            }

            foreach (var dto in page.PrescriptionTemplates)
            {
                await UpsertPrescriptionTemplateAsync(dto, cancellationToken);
            }

            foreach (var dto in page.ClinicalExams)
            {
                await UpsertClinicalExamAsync(dto, cancellationToken);
            }

            foreach (var dto in page.IssuedPrescriptions)
            {
                await UpsertIssuedPrescriptionAsync(dto, cancellationToken);
            }

            foreach (var dto in page.ClinicalAttachments)
            {
                await UpsertClinicalAttachmentAsync(dto, cancellationToken);
            }

            foreach (var dto in page.VaccineProtocols)
            {
                await UpsertVaccineProtocolAsync(dto, cancellationToken);
            }

            foreach (var dto in page.VaccineDoses)
            {
                await UpsertVaccineDoseAsync(dto, cancellationToken);
            }

            foreach (var dto in page.ClinicalQuotes)
            {
                await UpsertClinicalQuoteAsync(dto, cancellationToken);
            }

            foreach (var dto in page.WardUnits)
            {
                await UpsertWardUnitAsync(dto, cancellationToken);
            }

            foreach (var dto in page.Hospitalizations)
            {
                await UpsertHospitalizationAsync(dto, cancellationToken);
            }

            foreach (var dto in page.InventorySuppliers)
            {
                await UpsertInventorySupplierAsync(dto, cancellationToken);
            }

            foreach (var dto in page.InventoryProducts)
            {
                await UpsertInventoryProductAsync(dto, cancellationToken);
            }

            foreach (var dto in page.InventoryProductLots)
            {
                await UpsertInventoryProductLotAsync(dto, cancellationToken);
            }

            var state = await _dbContext.SyncState.FindAsync([1], cancellationToken)
                        ?? _dbContext.SyncState.Add(new SyncState()).Entity;
            state.LastPullAt = page.NextSince;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _dbContext.SuppressOutbox = false;
        }
    }

    private async Task UpsertTutorAsync(ClientSyncTutorDto dto, CancellationToken cancellationToken)
    {
        var tutor = await _dbContext.Tutors.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == dto.Id, cancellationToken);
        if (tutor is null)
        {
            var email = Email.Create(dto.Email);
            var cpf = Cpf.Create(dto.Cpf);
            var phone = Phone.Create(dto.Phone);
            if (email.IsFailure || cpf.IsFailure || phone.IsFailure)
            {
                return;
            }

            var created = Tutor.Create(dto.Name, email.Value, cpf.Value, phone.Value, dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            tutor = created.Value;
            tutor.UpdatedAt = dto.UpdatedAt;
            if (dto.IsDeleted)
            {
                tutor.SoftDelete();
            }

            _dbContext.Tutors.Add(tutor);
            return;
        }

        if (dto.UpdatedAt <= tutor.UpdatedAt)
        {
            return;
        }

        if (dto.IsDeleted)
        {
            tutor.SoftDelete();
            tutor.UpdatedAt = dto.UpdatedAt;
            return;
        }

        var emailResult = Email.Create(dto.Email);
        var phoneResult = Phone.Create(dto.Phone);
        if (emailResult.IsFailure || phoneResult.IsFailure)
        {
            return;
        }

        tutor.Update(dto.Name, emailResult.Value, phoneResult.Value);
        tutor.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertPetAsync(ClientSyncPetDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PetSpecies>(dto.Species, out var species))
        {
            return;
        }

        if (!Enum.TryParse<PetSex>(dto.Sex, out var sex))
        {
            return;
        }

        var pet = await _dbContext.Pets.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
        if (pet is null)
        {
            var created = Pet.Create(dto.Name, species, dto.Breed, sex, dto.TutorId, dto.Id, dto.BirthDate);
            if (created.IsFailure)
            {
                return;
            }

            pet = created.Value;
            pet.UpdatedAt = dto.UpdatedAt;
            if (dto.IsDeleted)
            {
                pet.SoftDelete();
            }

            _dbContext.Pets.Add(pet);
            return;
        }

        if (dto.UpdatedAt <= pet.UpdatedAt)
        {
            return;
        }

        if (dto.IsDeleted)
        {
            pet.SoftDelete();
            pet.UpdatedAt = dto.UpdatedAt;
            return;
        }

        pet.Update(dto.Name, species, dto.Breed, sex, dto.BirthDate);
        pet.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertAppointmentAsync(ClientSyncAppointmentDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AppointmentStatus>(dto.Status, out var status))
        {
            return;
        }

        var appointment = await _dbContext.Appointments.FindAsync([dto.Id], cancellationToken);
        if (appointment is null)
        {
            var created = Appointment.RestoreFromSync(
                dto.Id,
                dto.TutorId,
                dto.PetId,
                dto.VeterinarianId,
                dto.Date,
                dto.DurationInMinutes,
                dto.Reason,
                status,
                dto.UpdatedAt);
            _dbContext.Appointments.Add(created);
            return;
        }

        if (dto.UpdatedAt <= appointment.UpdatedAt)
        {
            return;
        }

        appointment.ApplySyncSnapshot(dto.Date, dto.DurationInMinutes, dto.Reason, status, dto.UpdatedAt);
    }

    private async Task UpsertScheduleSlotAsync(ClientSyncScheduleSlotDto dto, CancellationToken cancellationToken)
    {
        var slot = await _dbContext.ScheduleSlots.FindAsync([dto.Id], cancellationToken);
        if (slot is null)
        {
            slot = new ScheduleSlot(dto.Id, dto.VeterinarianId, dto.Date, dto.StartTime, dto.EndTime);
            slot.UpdatedAt = dto.UpdatedAt;
            if (!dto.IsAvailable)
            {
                slot.Block();
            }

            _dbContext.ScheduleSlots.Add(slot);
            return;
        }

        if (dto.UpdatedAt <= slot.UpdatedAt)
        {
            return;
        }

        if (dto.IsAvailable)
        {
            slot.Unblock();
        }
        else if (slot.IsAvailable)
        {
            slot.Block();
        }

        slot.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertMedicalRecordAsync(ClientSyncMedicalRecordDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MedicalRecordStatus>(dto.Status, out var status))
        {
            return;
        }

        VitalSigns? vitalSigns = null;
        if (dto.VitalWeightKg is not null && dto.VitalTemperatureC is not null && dto.VitalMeasuredAt is not null)
        {
            var vitalsResult = VitalSigns.Create(
                dto.VitalWeightKg.Value,
                dto.VitalTemperatureC.Value,
                dto.VitalHeartRateBpm,
                dto.VitalRespiratoryRateBpm,
                dto.VitalMucousMembranes ?? string.Empty,
                dto.VitalCapillaryRefillTime ?? string.Empty,
                dto.VitalMeasuredAt.Value);
            if (vitalsResult.IsSuccess)
            {
                vitalSigns = vitalsResult.Value;
            }
        }

        var evolution = dto.EvolutionNotes.Select(n => (n.Id, n.AuthorId, n.Text, n.RecordedAt));

        var record = await _dbContext.MedicalRecords
            .Include(r => r.EvolutionNotes)
            .FirstOrDefaultAsync(r => r.Id == dto.Id, cancellationToken);

        if (record is null)
        {
            var created = MedicalRecord.RestoreFromSync(
                dto.Id,
                dto.AppointmentId,
                dto.VeterinarianId,
                dto.TutorId,
                dto.PetId,
                dto.Anamnesis,
                dto.Diagnosis,
                dto.Prescription,
                status,
                dto.UpdatedAt,
                vitalSigns,
                evolution);
            _dbContext.MedicalRecords.Add(created);
            return;
        }

        if (dto.UpdatedAt <= record.UpdatedAt)
        {
            return;
        }

        record.ApplySyncSnapshot(dto.Anamnesis, dto.Diagnosis, dto.Prescription, status, vitalSigns, dto.UpdatedAt, evolution);
    }

    private async Task UpsertPrescriptionTemplateAsync(ClientSyncPrescriptionTemplateDto dto, CancellationToken cancellationToken)
    {
        var items = dto.Items.Select(i => (i.Id, i.MedicationName, i.Concentration, i.Dose, i.Route, i.Frequency, i.Duration, i.Instructions, i.SortOrder));
        var template = await _dbContext.PrescriptionTemplates.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == dto.Id, cancellationToken);
        if (template is null)
        {
            _dbContext.PrescriptionTemplates.Add(PrescriptionTemplate.RestoreFromSync(
                dto.Id, dto.Name, dto.Species, dto.IsActive, dto.UpdatedAt, items));
            return;
        }

        if (dto.UpdatedAt <= template.UpdatedAt)
        {
            return;
        }

        template.ApplySyncSnapshot(dto.Name, dto.Species, dto.IsActive, dto.UpdatedAt, items);
    }

    private async Task UpsertClinicalExamAsync(ClientSyncClinicalExamDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ClinicalExamCategory>(dto.Category, out var category) ||
            !Enum.TryParse<ClinicalExamStatus>(dto.Status, out var status))
        {
            return;
        }

        var exam = await _dbContext.ClinicalExams.FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
        if (exam is null)
        {
            _dbContext.ClinicalExams.Add(ClinicalExam.RestoreFromSync(
                dto.Id, dto.AppointmentId, dto.PetId, dto.Name, category, status, dto.ResultSummary, dto.UpdatedAt));
            return;
        }

        if (dto.UpdatedAt <= exam.UpdatedAt)
        {
            return;
        }

        exam.ApplySyncSnapshot(dto.AppointmentId, dto.PetId, dto.Name, category, status, dto.ResultSummary, dto.UpdatedAt);
    }

    private async Task UpsertIssuedPrescriptionAsync(ClientSyncIssuedPrescriptionDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IssuedPrescriptionStatus>(dto.Status, out var status))
        {
            return;
        }

        var items = dto.Items.Select(i => (i.Id, i.MedicationName, i.Concentration, i.Dose, i.Route, i.Frequency, i.Duration, i.Instructions, i.SortOrder));
        var prescription = await _dbContext.IssuedPrescriptions.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
        if (prescription is null)
        {
            _dbContext.IssuedPrescriptions.Add(IssuedPrescription.RestoreFromSync(
                dto.Id, dto.AppointmentId, dto.PetId, dto.VeterinarianId, dto.TemplateId, status, dto.UpdatedAt, items));
            return;
        }

        if (dto.UpdatedAt <= prescription.UpdatedAt)
        {
            return;
        }

        prescription.ApplySyncSnapshot(dto.AppointmentId, dto.PetId, dto.VeterinarianId, dto.TemplateId, status, dto.UpdatedAt, items);
    }

    private async Task UpsertClinicalAttachmentAsync(ClientSyncClinicalAttachmentDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ClinicalAttachmentKind>(dto.Kind, out var kind))
        {
            return;
        }

        var attachment = await _dbContext.ClinicalAttachments.FirstOrDefaultAsync(a => a.Id == dto.Id, cancellationToken);
        if (attachment is null)
        {
            _dbContext.ClinicalAttachments.Add(ClinicalAttachment.RestoreFromSync(
                dto.Id,
                dto.AppointmentId,
                dto.MedicalRecordId,
                dto.ClinicalExamId,
                dto.FileName,
                dto.ContentType,
                dto.SizeBytes,
                kind,
                dto.BlobKey,
                dto.IsDeleted,
                dto.UpdatedAt));
            return;
        }

        if (dto.UpdatedAt <= attachment.UpdatedAt)
        {
            return;
        }

        attachment.ApplySyncSnapshot(
            dto.AppointmentId,
            dto.MedicalRecordId,
            dto.ClinicalExamId,
            dto.FileName,
            dto.ContentType,
            dto.SizeBytes,
            kind,
            dto.BlobKey,
            dto.IsDeleted,
            dto.UpdatedAt);
    }

    private async Task UpsertClinicalQuoteAsync(ClientSyncClinicalQuoteDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ClinicalQuoteStatus>(dto.Status, out var status) ||
            !Enum.TryParse<QuoteConversionStatus>(dto.ConversionStatus, out var conversionStatus))
        {
            return;
        }

        var items = dto.Items.Select(i =>
        {
            Enum.TryParse<ClinicalQuoteItemKind>(i.Kind, out var kind);
            return (i.Id, i.Description, i.Quantity, i.UnitPrice, kind, i.ProductId, i.SortOrder);
        });

        var quote = await _dbContext.ClinicalQuotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == dto.Id, cancellationToken);
        if (quote is null)
        {
            _dbContext.ClinicalQuotes.Add(ClinicalQuote.RestoreFromSync(
                dto.Id,
                dto.AppointmentId,
                dto.PetId,
                dto.TutorId,
                dto.CreatedByUserId,
                status,
                conversionStatus,
                dto.ConvertedOrderId,
                dto.Notes,
                dto.SentAt,
                dto.DecidedAt,
                dto.UpdatedAt,
                items));
            return;
        }

        if (dto.UpdatedAt <= quote.UpdatedAt)
        {
            return;
        }

        quote.ApplySyncSnapshot(
            dto.AppointmentId,
            dto.PetId,
            dto.TutorId,
            dto.CreatedByUserId,
            status,
            conversionStatus,
            dto.ConvertedOrderId,
            dto.Notes,
            dto.SentAt,
            dto.DecidedAt,
            dto.UpdatedAt,
            items);
    }

    private async Task UpsertVaccineProtocolAsync(ClientSyncVaccineProtocolDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PetSpecies>(dto.Species, out var species))
        {
            return;
        }

        var doses = dto.Doses.Select(d => (d.Id, d.Sequence, d.Label, d.MinAgeInDays, d.MaxAgeInDays, d.IntervalFromPreviousInDays, d.NextDoseIntervalInDays));
        var protocol = await _dbContext.VaccineProtocols.Include(p => p.Doses).FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
        if (protocol is null)
        {
            _dbContext.VaccineProtocols.Add(VaccineProtocol.RestoreFromSync(
                dto.Id, dto.Name, species, dto.IsActive, dto.UpdatedAt, doses));
            return;
        }

        if (dto.UpdatedAt <= protocol.UpdatedAt)
        {
            return;
        }

        protocol.ApplySyncSnapshot(dto.Name, species, dto.IsActive, dto.UpdatedAt, doses);
    }

    private async Task UpsertVaccineDoseAsync(ClientSyncVaccineDoseDto dto, CancellationToken cancellationToken)
    {
        var dose = await _dbContext.VaccineDoses.FirstOrDefaultAsync(d => d.Id == dto.Id, cancellationToken);
        if (dose is null)
        {
            _dbContext.VaccineDoses.Add(VaccineDose.RestoreFromSync(
                dto.Id,
                dto.PetId,
                dto.Name,
                dto.BatchNumber,
                dto.AppliedAt,
                dto.NextDueDate,
                dto.ProtocolId,
                dto.ProtocolDoseId,
                dto.UpdatedAt));
            return;
        }

        if (dto.UpdatedAt <= dose.UpdatedAt)
        {
            return;
        }

        dose.ApplySyncSnapshot(
            dto.Name,
            dto.BatchNumber,
            dto.AppliedAt,
            dto.NextDueDate,
            dto.ProtocolId,
            dto.ProtocolDoseId,
            dto.UpdatedAt);
    }

    private async Task UpsertWardUnitAsync(ClientSyncWardUnitDto dto, CancellationToken cancellationToken)
    {
        var beds = dto.Beds.Select(b => (b.Id, b.Code, b.SortOrder, b.IsActive));
        var unit = await _dbContext.WardUnits.Include(u => u.Beds).FirstOrDefaultAsync(u => u.Id == dto.Id, cancellationToken);
        if (unit is null)
        {
            _dbContext.WardUnits.Add(WardUnit.RestoreFromSync(dto.Id, dto.Name, dto.IsActive, dto.UpdatedAt, beds));
            return;
        }

        if (dto.UpdatedAt <= unit.UpdatedAt)
        {
            return;
        }

        unit.ApplySyncSnapshot(dto.Name, dto.IsActive, dto.UpdatedAt, beds);
    }

    private async Task UpsertHospitalizationAsync(ClientSyncHospitalizationDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HospitalizationStatus>(dto.Status, out var status))
        {
            return;
        }

        var orders = dto.MedicationOrders.Select(o =>
        {
            Enum.TryParse<HospitalMedicationOrderStatus>(o.Status, out var orderStatus);
            return (o.Id, o.MedicationName, o.Dose, o.Route, o.DailyTimesCsv, o.StartsOn, o.EndsOn, orderStatus, o.UpdatedAt);
        });
        var admins = dto.Administrations.Select(a =>
        {
            Enum.TryParse<MedicationAdministrationStatus>(a.Status, out var adminStatus);
            return (a.Id, a.MedicationOrderId, a.ScheduledAt, adminStatus, a.ActorId, a.ActedAt, a.Notes, a.UpdatedAt);
        });

        var hosp = await _dbContext.Hospitalizations
            .Include(h => h.MedicationOrders)
            .Include(h => h.Administrations)
            .FirstOrDefaultAsync(h => h.Id == dto.Id, cancellationToken);

        if (hosp is null)
        {
            _dbContext.Hospitalizations.Add(Hospitalization.RestoreFromSync(
                dto.Id,
                dto.PetId,
                dto.VeterinarianId,
                dto.BedId,
                dto.Reason,
                dto.AdmittedAt,
                dto.DischargedAt,
                status,
                dto.UpdatedAt,
                orders,
                admins,
                Array.Empty<(Guid, Guid, string, DateTimeOffset, DateTimeOffset)>(),
                Array.Empty<(Guid, string, Guid, DateTimeOffset, string, DateTimeOffset)>()));
            return;
        }

        if (dto.UpdatedAt <= hosp.UpdatedAt)
        {
            return;
        }

        hosp.ApplySyncSnapshot(
            dto.PetId,
            dto.VeterinarianId,
            dto.BedId,
            dto.Reason,
            dto.AdmittedAt,
            dto.DischargedAt,
            status,
            dto.UpdatedAt,
            orders,
            admins,
            Array.Empty<(Guid, Guid, string, DateTimeOffset, DateTimeOffset)>(),
            Array.Empty<(Guid, string, Guid, DateTimeOffset, string, DateTimeOffset)>());
    }

    private async Task UpsertInventorySupplierAsync(ClientSyncInventorySupplierDto dto, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.Id, cancellationToken);
        if (supplier is null)
        {
            var created = Supplier.Create(dto.LegalName, dto.TradeName, dto.Document, dto.ContactEmail, dto.ContactPhone, dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            supplier = created.Value;
            supplier.SetActive(dto.IsActive);
            supplier.UpdatedAt = dto.UpdatedAt;
            _dbContext.Suppliers.Add(supplier);
            return;
        }

        if (dto.UpdatedAt <= supplier.UpdatedAt)
        {
            return;
        }

        supplier.Update(dto.LegalName, dto.TradeName, dto.ContactEmail, dto.ContactPhone);
        supplier.SetActive(dto.IsActive);
        supplier.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertInventoryProductAsync(ClientSyncInventoryProductDto dto, CancellationToken cancellationToken)
    {
        Enum.TryParse<ProductCategory>(dto.Category, true, out var category);
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
        if (product is null)
        {
            var created = Product.Create(
                dto.Name,
                dto.Description,
                dto.Sku,
                dto.Barcode,
                dto.UnitOfMeasure,
                dto.ReorderLevel,
                category,
                dto.Ncm,
                dto.Cest,
                dto.MerchandiseOrigin,
                dto.SupplierId,
                dto.RequiresLot,
                dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            product = created.Value;
            product.SetActive(dto.IsActive);
            product.RecalculateAverageCost(dto.AverageCost);
            product.UpdatedAt = dto.UpdatedAt;
            _dbContext.Products.Add(product);
            await _dbContext.ProductBalances.AddAsync(new ProductBalance(dto.Id, 0m), cancellationToken);
            return;
        }

        if (dto.UpdatedAt <= product.UpdatedAt)
        {
            return;
        }

        product.UpdateDetails(
            dto.Name,
            dto.Description,
            dto.Sku,
            dto.Barcode,
            dto.UnitOfMeasure,
            dto.ReorderLevel,
            category,
            dto.Ncm,
            dto.Cest,
            dto.MerchandiseOrigin,
            dto.SupplierId,
            dto.RequiresLot);
        product.SetActive(dto.IsActive);
        product.RecalculateAverageCost(dto.AverageCost);
        product.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertInventoryProductLotAsync(ClientSyncInventoryProductLotDto dto, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.ProductLots.FirstOrDefaultAsync(l => l.Id == dto.Id, cancellationToken);
        if (lot is null)
        {
            var created = ProductLot.Create(dto.ProductId, dto.LotNumber, dto.ExpirationDate, dto.UnitCost, dto.Quantity, dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            lot = created.Value;
            lot.SetActive(dto.IsActive);
            lot.UpdatedAt = dto.UpdatedAt;
            _dbContext.ProductLots.Add(lot);
            return;
        }

        if (dto.UpdatedAt <= lot.UpdatedAt)
        {
            return;
        }

        lot.UpdateMetadata(dto.ExpirationDate, dto.UnitCost);
        lot.SetQuantity(dto.Quantity);
        lot.SetActive(dto.IsActive);
        lot.UpdatedAt = dto.UpdatedAt;
    }
}
