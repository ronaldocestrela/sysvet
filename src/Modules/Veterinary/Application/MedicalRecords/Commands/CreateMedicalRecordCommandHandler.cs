using Core.Domain;
using Core.Domain.Auditing;
using MediatR;
using Veterinary.Application.MedicalRecords;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.MedicalRecords.Commands;

/// <summary>Creates a draft medical record when the appointment is in progress or completed.</summary>
public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public CreateMedicalRecordCommandHandler(
        IAppointmentRepository appointmentRepository,
        IMedicalRecordRepository medicalRecordRepository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _appointmentRepository = appointmentRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);

        if (appointment == null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.MedicalRecord.AppointmentNotEligible);
        }

        var existing = await _medicalRecordRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var medicalRecordResult = MedicalRecord.Create(
            Guid.NewGuid(),
            appointment.Id,
            appointment.VeterinarianId,
            appointment.TutorId,
            appointment.PetId);

        if (medicalRecordResult.IsFailure)
        {
            return Result.Failure<Guid>(medicalRecordResult.Error);
        }

        var medicalRecord = medicalRecordResult.Value;

        await _medicalRecordRepository.AddAsync(medicalRecord, cancellationToken);

        await MedicalRecordAuditHelper.LogAsync(
            _auditLogger,
            _tenantContext,
            medicalRecord.Id,
            "Create",
            $"appointment={appointment.Id:N}, status=Draft",
            cancellationToken);

        return Result.Success(medicalRecord.Id);
    }
}
