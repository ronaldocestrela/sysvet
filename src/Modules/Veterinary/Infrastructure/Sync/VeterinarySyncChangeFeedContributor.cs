using Core.Application.Sync;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Infrastructure.Persistence;

namespace Veterinary.Infrastructure.Sync;

/// <summary>
/// Exposes veterinary agenda rows to the Core sync pull feed.
/// </summary>
public sealed class VeterinarySyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly VeterinaryDbContext _dbContext;

    /// <summary>Creates the contributor.</summary>
    public VeterinarySyncChangeFeedContributor(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.Appointments.AsNoTracking().ToListAsync(cancellationToken);
        var appointmentCandidates = appointments
            .Where(a => a.UpdatedAt > since)
            .OrderBy(a => a.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreAppointments = appointmentCandidates.Count > take;
        if (hasMoreAppointments)
        {
            appointmentCandidates = appointmentCandidates.Take(take).ToList();
        }

        var slots = await _dbContext.ScheduleSlots.AsNoTracking().ToListAsync(cancellationToken);
        var slotCandidates = slots
            .Where(s => s.UpdatedAt > since)
            .OrderBy(s => s.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreSlots = slotCandidates.Count > take;
        if (hasMoreSlots)
        {
            slotCandidates = slotCandidates.Take(take).ToList();
        }

        var maxUpdated = since;
        foreach (var a in appointmentCandidates)
        {
            if (a.UpdatedAt > maxUpdated)
            {
                maxUpdated = a.UpdatedAt;
            }
        }

        foreach (var s in slotCandidates)
        {
            if (s.UpdatedAt > maxUpdated)
            {
                maxUpdated = s.UpdatedAt;
            }
        }

        var medicalRecords = await _dbContext.MedicalRecords
            .AsNoTracking()
            .Include(m => m.EvolutionNotes)
            .ToListAsync(cancellationToken);
        var recordCandidates = medicalRecords
            .Where(r => r.UpdatedAt > since)
            .OrderBy(r => r.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreRecords = recordCandidates.Count > take;
        if (hasMoreRecords)
        {
            recordCandidates = recordCandidates.Take(take).ToList();
        }

        foreach (var r in recordCandidates)
        {
            if (r.UpdatedAt > maxUpdated)
            {
                maxUpdated = r.UpdatedAt;
            }
        }

        var templates = await _dbContext.PrescriptionTemplates.AsNoTracking().Include(t => t.Items).ToListAsync(cancellationToken);
        var templateCandidates = templates.Where(t => t.UpdatedAt > since).OrderBy(t => t.UpdatedAt).Take(take + 1).ToList();
        var hasMoreTemplates = templateCandidates.Count > take;
        if (hasMoreTemplates)
        {
            templateCandidates = templateCandidates.Take(take).ToList();
        }

        var prescriptions = await _dbContext.IssuedPrescriptions.AsNoTracking().Include(p => p.Items).ToListAsync(cancellationToken);
        var prescriptionCandidates = prescriptions.Where(p => p.UpdatedAt > since).OrderBy(p => p.UpdatedAt).Take(take + 1).ToList();
        var hasMorePrescriptions = prescriptionCandidates.Count > take;
        if (hasMorePrescriptions)
        {
            prescriptionCandidates = prescriptionCandidates.Take(take).ToList();
        }

        var exams = await _dbContext.ClinicalExams.AsNoTracking().ToListAsync(cancellationToken);
        var examCandidates = exams.Where(e => e.UpdatedAt > since).OrderBy(e => e.UpdatedAt).Take(take + 1).ToList();
        var hasMoreExams = examCandidates.Count > take;
        if (hasMoreExams)
        {
            examCandidates = examCandidates.Take(take).ToList();
        }

        var attachments = await _dbContext.ClinicalAttachments.AsNoTracking().ToListAsync(cancellationToken);
        var attachmentCandidates = attachments.Where(a => a.UpdatedAt > since).OrderBy(a => a.UpdatedAt).Take(take + 1).ToList();
        var hasMoreAttachments = attachmentCandidates.Count > take;
        if (hasMoreAttachments)
        {
            attachmentCandidates = attachmentCandidates.Take(take).ToList();
        }

        var protocols = await _dbContext.VaccineProtocols.AsNoTracking().Include(p => p.Doses).ToListAsync(cancellationToken);
        var protocolCandidates = protocols.Where(p => p.UpdatedAt > since).OrderBy(p => p.UpdatedAt).Take(take + 1).ToList();
        var hasMoreProtocols = protocolCandidates.Count > take;
        if (hasMoreProtocols)
        {
            protocolCandidates = protocolCandidates.Take(take).ToList();
        }

        var vaccineDoses = await _dbContext.VaccineDoses.AsNoTracking().ToListAsync(cancellationToken);
        var doseCandidates = vaccineDoses.Where(v => v.UpdatedAt > since).OrderBy(v => v.UpdatedAt).Take(take + 1).ToList();
        var hasMoreDoses = doseCandidates.Count > take;
        if (hasMoreDoses)
        {
            doseCandidates = doseCandidates.Take(take).ToList();
        }

        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking().Include(q => q.Items).ToListAsync(cancellationToken);
        var quoteCandidates = quotes.Where(q => q.UpdatedAt > since).OrderBy(q => q.UpdatedAt).Take(take + 1).ToList();
        var hasMoreQuotes = quoteCandidates.Count > take;
        if (hasMoreQuotes)
        {
            quoteCandidates = quoteCandidates.Take(take).ToList();
        }

        foreach (var updatedAt in templateCandidates.Select(t => t.UpdatedAt)
                     .Concat(prescriptionCandidates.Select(p => p.UpdatedAt))
                     .Concat(examCandidates.Select(e => e.UpdatedAt))
                     .Concat(attachmentCandidates.Select(a => a.UpdatedAt))
                     .Concat(protocolCandidates.Select(p => p.UpdatedAt))
                     .Concat(doseCandidates.Select(d => d.UpdatedAt))
                     .Concat(quoteCandidates.Select(q => q.UpdatedAt)))
        {
            if (updatedAt > maxUpdated)
            {
                maxUpdated = updatedAt;
            }
        }

        return new SyncContributorChanges
        {
            Appointments = appointmentCandidates.Select(MapAppointment).ToList(),
            ScheduleSlots = slotCandidates.Select(MapSlot).ToList(),
            MedicalRecords = recordCandidates.Select(MapMedicalRecord).ToList(),
            PrescriptionTemplates = templateCandidates.Select(MapTemplate).ToList(),
            IssuedPrescriptions = prescriptionCandidates.Select(MapIssuedPrescription).ToList(),
            ClinicalExams = examCandidates.Select(MapExam).ToList(),
            ClinicalAttachments = attachmentCandidates.Select(MapAttachment).ToList(),
            VaccineProtocols = protocolCandidates.Select(MapVaccineProtocol).ToList(),
            VaccineDoses = doseCandidates.Select(MapVaccineDose).ToList(),
            ClinicalQuotes = quoteCandidates.Select(MapClinicalQuote).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMoreAppointments || hasMoreSlots || hasMoreRecords || hasMoreTemplates || hasMorePrescriptions || hasMoreExams || hasMoreAttachments || hasMoreProtocols || hasMoreDoses || hasMoreQuotes
        };
    }

    private static SyncVaccineProtocolDto MapVaccineProtocol(VaccineProtocol protocol) =>
        new()
        {
            Id = protocol.Id,
            Name = protocol.Name,
            Species = protocol.Species.ToString(),
            IsActive = protocol.IsActive,
            Doses = protocol.Doses.OrderBy(d => d.Sequence).Select(d => new SyncVaccineProtocolDoseDto
            {
                Id = d.Id,
                Sequence = d.Sequence,
                Label = d.Label,
                MinAgeInDays = d.MinAgeInDays,
                MaxAgeInDays = d.MaxAgeInDays,
                IntervalFromPreviousInDays = d.IntervalFromPreviousInDays,
                NextDoseIntervalInDays = d.NextDoseIntervalInDays
            }).ToList(),
            UpdatedAt = protocol.UpdatedAt,
            RowVersion = Convert.ToBase64String(protocol.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncVaccineDoseDto MapVaccineDose(VaccineDose dose) =>
        new()
        {
            Id = dose.Id,
            PetId = dose.PetId,
            Name = dose.Name,
            BatchNumber = dose.BatchNumber,
            AppliedAt = dose.AppliedAt,
            NextDueDate = dose.NextDueDate,
            ProtocolId = dose.ProtocolId,
            ProtocolDoseId = dose.ProtocolDoseId,
            UpdatedAt = dose.UpdatedAt,
            RowVersion = Convert.ToBase64String(dose.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncAppointmentDto MapAppointment(Appointment appointment) =>
        new()
        {
            Id = appointment.Id,
            TutorId = appointment.TutorId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Date = appointment.Date,
            DurationInMinutes = appointment.DurationInMinutes,
            Reason = appointment.Reason,
            Status = appointment.Status.ToString(),
            UpdatedAt = appointment.UpdatedAt,
            RowVersion = Convert.ToBase64String(appointment.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncScheduleSlotDto MapSlot(ScheduleSlot slot) =>
        new()
        {
            Id = slot.Id,
            VeterinarianId = slot.VeterinarianId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            IsAvailable = slot.IsAvailable,
            UpdatedAt = slot.UpdatedAt,
            RowVersion = Convert.ToBase64String(slot.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncMedicalRecordDto MapMedicalRecord(MedicalRecord record) =>
        new()
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            VeterinarianId = record.VeterinarianId,
            TutorId = record.TutorId,
            PetId = record.PetId,
            Anamnesis = record.Anamnesis,
            Diagnosis = record.Diagnosis,
            Prescription = record.Prescription,
            Status = record.Status.ToString(),
            VitalWeightKg = record.VitalSigns?.WeightKg,
            VitalTemperatureC = record.VitalSigns?.TemperatureC,
            VitalHeartRateBpm = record.VitalSigns?.HeartRateBpm,
            VitalRespiratoryRateBpm = record.VitalSigns?.RespiratoryRateBpm,
            VitalMucousMembranes = record.VitalSigns?.MucousMembranes,
            VitalCapillaryRefillTime = record.VitalSigns?.CapillaryRefillTime,
            VitalMeasuredAt = record.VitalSigns?.MeasuredAt,
            EvolutionNotes = record.EvolutionNotes.Select(n => new SyncEvolutionNoteDto
            {
                Id = n.Id,
                AuthorId = n.AuthorId,
                Text = n.Text,
                RecordedAt = n.RecordedAt
            }).ToList(),
            UpdatedAt = record.UpdatedAt,
            RowVersion = Convert.ToBase64String(record.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncPrescriptionTemplateDto MapTemplate(PrescriptionTemplate template) =>
        new()
        {
            Id = template.Id,
            Name = template.Name,
            Species = template.Species,
            IsActive = template.IsActive,
            Items = template.Items.OrderBy(i => i.SortOrder).Select(i => new SyncPrescriptionLineDto
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                Concentration = i.Concentration,
                Dose = i.Dose,
                Route = i.Route,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Instructions = i.Instructions,
                SortOrder = i.SortOrder
            }).ToList(),
            UpdatedAt = template.UpdatedAt,
            RowVersion = Convert.ToBase64String(template.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncIssuedPrescriptionDto MapIssuedPrescription(IssuedPrescription prescription) =>
        new()
        {
            Id = prescription.Id,
            AppointmentId = prescription.AppointmentId,
            PetId = prescription.PetId,
            VeterinarianId = prescription.VeterinarianId,
            TemplateId = prescription.TemplateId,
            Status = prescription.Status.ToString(),
            Items = prescription.Items.OrderBy(i => i.SortOrder).Select(i => new SyncPrescriptionLineDto
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                Concentration = i.Concentration,
                Dose = i.Dose,
                Route = i.Route,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Instructions = i.Instructions,
                SortOrder = i.SortOrder
            }).ToList(),
            UpdatedAt = prescription.UpdatedAt,
            RowVersion = Convert.ToBase64String(prescription.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncClinicalExamDto MapExam(ClinicalExam exam) =>
        new()
        {
            Id = exam.Id,
            AppointmentId = exam.AppointmentId,
            PetId = exam.PetId,
            Name = exam.Name,
            Category = exam.Category.ToString(),
            Status = exam.Status.ToString(),
            ResultSummary = exam.ResultSummary,
            UpdatedAt = exam.UpdatedAt,
            RowVersion = Convert.ToBase64String(exam.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncClinicalQuoteDto MapClinicalQuote(ClinicalQuote quote) =>
        new()
        {
            Id = quote.Id,
            AppointmentId = quote.AppointmentId,
            PetId = quote.PetId,
            TutorId = quote.TutorId,
            CreatedByUserId = quote.CreatedByUserId,
            Status = quote.Status.ToString(),
            ConversionStatus = quote.ConversionStatus.ToString(),
            ConvertedOrderId = quote.ConvertedOrderId,
            Notes = quote.Notes,
            SentAt = quote.SentAt,
            DecidedAt = quote.DecidedAt,
            Items = quote.Items.OrderBy(i => i.SortOrder).Select(i => new SyncClinicalQuoteItemDto
            {
                Id = i.Id,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Kind = i.Kind.ToString(),
                ProductId = i.ProductId,
                SortOrder = i.SortOrder
            }).ToList(),
            UpdatedAt = quote.UpdatedAt,
            RowVersion = Convert.ToBase64String(quote.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncClinicalAttachmentDto MapAttachment(ClinicalAttachment attachment) =>
        new()
        {
            Id = attachment.Id,
            AppointmentId = attachment.AppointmentId,
            MedicalRecordId = attachment.MedicalRecordId,
            ClinicalExamId = attachment.ClinicalExamId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.SizeBytes,
            Kind = attachment.Kind.ToString(),
            BlobKey = attachment.BlobKey,
            IsDeleted = attachment.IsDeleted,
            UpdatedAt = attachment.UpdatedAt,
            RowVersion = Convert.ToBase64String(attachment.RowVersion ?? Array.Empty<byte>())
        };
}
