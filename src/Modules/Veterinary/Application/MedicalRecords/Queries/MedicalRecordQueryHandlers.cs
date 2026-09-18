using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.MedicalRecords.DTOs;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.MedicalRecords.Queries;

/// <summary>Resolves a medical record by id.</summary>
public sealed class GetMedicalRecordByIdQueryHandler : IRequestHandler<GetMedicalRecordByIdQuery, Result<MedicalRecordDto>>
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetMedicalRecordByIdQueryHandler(IMedicalRecordRepository medicalRecordRepository, IAppointmentRepository appointmentRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<MedicalRecordDto>> Handle(GetMedicalRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _medicalRecordRepository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null)
        {
            return Result.Failure<MedicalRecordDto>(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var appointment = await _appointmentRepository.GetByIdAsync(record.AppointmentId, cancellationToken);
        return Result.Success(MedicalRecordMapper.ToDto(record, appointment?.Date));
    }
}

/// <summary>Resolves a medical record by appointment id.</summary>
public sealed class GetMedicalRecordByAppointmentQueryHandler : IRequestHandler<GetMedicalRecordByAppointmentQuery, Result<MedicalRecordDto>>
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetMedicalRecordByAppointmentQueryHandler(IMedicalRecordRepository medicalRecordRepository, IAppointmentRepository appointmentRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<MedicalRecordDto>> Handle(GetMedicalRecordByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var record = await _medicalRecordRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<MedicalRecordDto>(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var appointment = await _appointmentRepository.GetByIdAsync(record.AppointmentId, cancellationToken);
        return Result.Success(MedicalRecordMapper.ToDto(record, appointment?.Date));
    }
}

/// <summary>Builds pet clinical timeline ordered by visit date.</summary>
public sealed class GetPetClinicalTimelineQueryHandler : IRequestHandler<GetPetClinicalTimelineQuery, Result<IReadOnlyList<PetClinicalTimelineItemDto>>>
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetPetClinicalTimelineQueryHandler(IMedicalRecordRepository medicalRecordRepository, IAppointmentRepository appointmentRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<IReadOnlyList<PetClinicalTimelineItemDto>>> Handle(GetPetClinicalTimelineQuery request, CancellationToken cancellationToken)
    {
        var records = await _medicalRecordRepository.GetByPetIdAsync(request.PetId, cancellationToken);
        var items = new List<PetClinicalTimelineItemDto>(records.Count);

        foreach (var record in records)
        {
            var occurredAt = record.UpdatedAt;
            var appointment = await _appointmentRepository.GetByIdAsync(record.AppointmentId, cancellationToken);
            if (appointment is not null)
            {
                occurredAt = appointment.Date;
            }

            items.Add(MedicalRecordMapper.ToTimelineItem(record, occurredAt));
        }

        items.Sort((a, b) => b.OccurredAt.CompareTo(a.OccurredAt));
        return Result.Success<IReadOnlyList<PetClinicalTimelineItemDto>>(items);
    }
}
