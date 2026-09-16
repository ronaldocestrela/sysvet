using Core.Domain;
using MediatR;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.MedicalRecords.Commands;

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;

    public CreateMedicalRecordCommandHandler(
        IAppointmentRepository appointmentRepository,
        IMedicalRecordRepository medicalRecordRepository)
    {
        _appointmentRepository = appointmentRepository;
        _medicalRecordRepository = medicalRecordRepository;
    }

    public async Task<Result<Guid>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);

        if (appointment == null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
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

        return Result.Success(medicalRecord.Id);
    }
}
