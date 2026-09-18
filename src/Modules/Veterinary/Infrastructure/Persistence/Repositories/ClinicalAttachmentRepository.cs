using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class ClinicalAttachmentRepository : IClinicalAttachmentRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public ClinicalAttachmentRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(ClinicalAttachment attachment, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalAttachments.AddAsync(attachment, cancellationToken);

    public async Task<ClinicalAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalAttachments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ClinicalAttachment>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var attachments = await _dbContext.ClinicalAttachments.AsNoTracking()
            .Where(a => a.AppointmentId == appointmentId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
        return attachments.OrderByDescending(a => a.UpdatedAt).ToList();
    }

    public void Update(ClinicalAttachment attachment) => _dbContext.ClinicalAttachments.Update(attachment);
}
