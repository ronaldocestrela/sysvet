using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Application.GroomingAppointments;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingAppointments.Commands;

using ErrorCodes = Petshop.Domain.ErrorCodes;

/// <summary>
/// Books a grooming appointment and opens a draft digital record seeded from the service catalog.
/// </summary>
public sealed class ScheduleGroomingAppointmentCommandHandler : IRequestHandler<ScheduleGroomingAppointmentCommand, Result<Guid>>
{
    private readonly IGroomingAppointmentRepository _appointmentRepository;
    private readonly IGroomingSlotRepository _slotRepository;
    private readonly IGroomingServiceRepository _serviceRepository;
    private readonly IGroomingRecordRepository _recordRepository;

    public ScheduleGroomingAppointmentCommandHandler(
        IGroomingAppointmentRepository appointmentRepository,
        IGroomingSlotRepository slotRepository,
        IGroomingServiceRepository serviceRepository,
        IGroomingRecordRepository recordRepository)
    {
        _appointmentRepository = appointmentRepository;
        _slotRepository = slotRepository;
        _serviceRepository = serviceRepository;
        _recordRepository = recordRepository;
    }

    public async Task<Result<Guid>> Handle(ScheduleGroomingAppointmentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var service = await _serviceRepository.GetByIdAsync(request.GroomingServiceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingService.NotFound);
        }

        var duration = request.DurationInMinutes > 0 ? request.DurationInMinutes : service.DurationInMinutes;

        if (await _appointmentRepository.HasOverlappingAsync(
                request.GroomerId,
                request.Date,
                duration,
                null,
                cancellationToken))
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingAppointment.Overlap);
        }

        var availableSlots = await _slotRepository.GetAvailableSlotsAsync(request.GroomerId, request.Date, cancellationToken);
        var slot = GroomingSlotHelper.FindCoveringSlot(availableSlots, request.Date, duration);
        if (slot is null)
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingAppointment.SlotUnavailable);
        }

        var bookResult = slot.Book();
        if (bookResult.IsFailure)
        {
            return Result.Failure<Guid>(bookResult.Error);
        }

        _slotRepository.Update(slot);

        var appointmentResult = GroomingAppointment.Create(
            request.Id,
            request.TutorId,
            request.PetId,
            request.GroomerId,
            request.GroomingServiceId,
            request.Date,
            duration,
            request.Notes);

        if (appointmentResult.IsFailure)
        {
            return Result.Failure<Guid>(appointmentResult.Error);
        }

        var recordResult = GroomingRecord.Create(Guid.NewGuid(), appointmentResult.Value.Id, request.GroomerId, request.TutorId, request.PetId);
        if (recordResult.IsFailure)
        {
            return Result.Failure<Guid>(recordResult.Error);
        }

        var seed = recordResult.Value.SeedSuppliesFromService(service);
        if (seed.IsFailure)
        {
            return Result.Failure<Guid>(seed.Error);
        }

        await _appointmentRepository.AddAsync(appointmentResult.Value, cancellationToken);
        await _recordRepository.AddAsync(recordResult.Value, cancellationToken);
        return Result.Success(appointmentResult.Value.Id);
    }
}
