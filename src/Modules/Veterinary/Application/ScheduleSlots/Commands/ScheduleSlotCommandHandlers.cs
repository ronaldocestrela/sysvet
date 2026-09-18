using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.ScheduleSlots.Commands;

public sealed class DefineDailyAvailabilityCommandHandler : IRequestHandler<DefineDailyAvailabilityCommand, Result>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public DefineDailyAvailabilityCommandHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result> Handle(DefineDailyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (request.SlotDurationMinutes <= 0 || request.DayEnd <= request.DayStart)
        {
            return Result.Failure(new Error("ScheduleSlot.InvalidRange", "Invalid availability range or slot duration."));
        }

        var existing = await _scheduleSlotRepository.GetAllSlotsForDayAsync(
            request.VeterinarianId,
            request.Date,
            cancellationToken);

        if (existing.Any())
        {
            return Result.Success();
        }

        var slots = new List<ScheduleSlot>();
        var cursor = request.DayStart;
        var dayDate = request.Date.UtcDateTime.Date;

        while (cursor.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes)) <= request.DayEnd)
        {
            var end = cursor.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes));
            slots.Add(new ScheduleSlot(
                Guid.NewGuid(),
                request.VeterinarianId,
                new DateTimeOffset(dayDate, TimeSpan.Zero),
                cursor,
                end));
            cursor = end;
        }

        await _scheduleSlotRepository.AddRangeAsync(slots, cancellationToken);
        return Result.Success();
    }
}

public sealed class BlockScheduleSlotCommandHandler : IRequestHandler<BlockScheduleSlotCommand, Result>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public BlockScheduleSlotCommandHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result> Handle(BlockScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleSlotRepository.GetByIdAsync(request.ScheduleSlotId, cancellationToken);
        if (slot is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.ScheduleSlot.NotFound);
        }

        var result = slot.Block();
        if (result.IsFailure)
        {
            return result;
        }

        _scheduleSlotRepository.Update(slot);
        return Result.Success();
    }
}

public sealed class UnblockScheduleSlotCommandHandler : IRequestHandler<UnblockScheduleSlotCommand, Result>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public UnblockScheduleSlotCommandHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result> Handle(UnblockScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleSlotRepository.GetByIdAsync(request.ScheduleSlotId, cancellationToken);
        if (slot is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.ScheduleSlot.NotFound);
        }

        var result = slot.Unblock();
        if (result.IsFailure)
        {
            return result;
        }

        _scheduleSlotRepository.Update(slot);
        return Result.Success();
    }
}
