using Core.Domain;
using Core.Domain.ValueObjects;
using MediatR;

namespace Core.Application.Tutors.Commands;

public class UpdateTutorCommandHandler : IRequestHandler<UpdateTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly Core.Domain.Auditing.IAuditLogger _auditLogger;
    private readonly Core.Domain.ITenantContext _tenantContext;

    public UpdateTutorCommandHandler(
        ITutorRepository tutorRepository,
        Core.Domain.Auditing.IAuditLogger auditLogger,
        Core.Domain.ITenantContext tenantContext)
    {
        _tutorRepository = tutorRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateTutorCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tutor == null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return emailResult;

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure) return phoneResult;

        var updateResult = tutor.Update(request.Name, emailResult.Value, phoneResult.Value);
        if (updateResult.IsFailure) return updateResult;

        _tutorRepository.Update(tutor);

        await _auditLogger.LogAsync(_tenantContext.TenantId, Guid.Empty, "Tutor", "Update", $"Tutor {request.Id} updated.", cancellationToken);

        return Result.Success();
    }
}
