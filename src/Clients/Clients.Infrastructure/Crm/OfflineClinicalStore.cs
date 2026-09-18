using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed clinical store with outbox for exam requests.</summary>
public sealed class OfflineClinicalStore : IClinicalStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineClinicalStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<ClinicalExamListItemDto>>> GetExamsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var exams = await _dbContext.ClinicalExams.AsNoTracking()
            .Where(e => e.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClinicalExamListItemDto>>(exams
            .OrderByDescending(e => e.UpdatedAt)
            .Select(e => new ClinicalExamListItemDto
        {
            Id = e.Id,
            Name = e.Name,
            Category = e.Category.ToString(),
            Status = e.Status.ToString(),
            ResultSummary = e.ResultSummary
        }).ToList());
    }

    public async Task<Result<Guid>> RequestExamAsync(Guid appointmentId, string name, string category, CancellationToken cancellationToken = default)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(new Error("Clinical.AppointmentNotFound", "Appointment not found locally."));
        }

        if (!Enum.TryParse<ClinicalExamCategory>(category, true, out var parsedCategory))
        {
            return Result.Failure<Guid>(new Error("Clinical.InvalidCategory", "Invalid exam category."));
        }

        var id = Guid.NewGuid();
        var created = ClinicalExam.Request(id, appointmentId, appointment.PetId, name, parsedCategory);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _dbContext.ClinicalExams.Add(created.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    public async Task<Result<IReadOnlyList<IssuedPrescriptionListItemDto>>> GetPrescriptionsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.IssuedPrescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<IssuedPrescriptionListItemDto>>(list
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new IssuedPrescriptionListItemDto
        {
            Id = p.Id,
            Status = p.Status.ToString(),
            ItemCount = p.Items.Count
        }).ToList());
    }

    public async Task<Result<IReadOnlyList<ClinicalAttachmentListItemDto>>> GetAttachmentsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.ClinicalAttachments.AsNoTracking()
            .Where(a => a.AppointmentId == appointmentId && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClinicalAttachmentListItemDto>>(list
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new ClinicalAttachmentListItemDto
        {
            Id = a.Id,
            FileName = a.FileName,
            Kind = a.Kind.ToString(),
            SizeBytes = a.SizeBytes
        }).ToList());
    }
}
