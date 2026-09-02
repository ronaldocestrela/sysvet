using Core.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.MedicalRecords.Commands;

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMedicalRecordCommandHandler(
        IAppointmentRepository appointmentRepository,
        IMedicalRecordRepository medicalRecordRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        
        if (appointment == null)
        {
            return Result.Failure<Guid>(new Error("Appointment.NotFound", "The specified appointment was not found."));
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(medicalRecord.Id);
    }
}
