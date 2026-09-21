using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingSlots.Commands;

using ErrorCodes = Petshop.Domain.ErrorCodes;

public sealed class DefineGroomingDailyAvailabilityCommandHandler : IRequestHandler<DefineGroomingDailyAvailabilityCommand, Result>
{
    private readonly IGroomingSlotRepository _slotRepository;

    public DefineGroomingDailyAvailabilityCommandHandler(IGroomingSlotRepository slotRepository) => _slotRepository = slotRepository;

    public async Task<Result> Handle(DefineGroomingDailyAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (request.SlotDurationMinutes <= 0 || request.DayEnd <= request.DayStart)
        {
            return Result.Failure(new Error("GroomingSlot.InvalidRange", "Invalid availability range or slot duration."));
        }

        var existing = await _slotRepository.GetAllSlotsForDayAsync(request.GroomerId, request.Date, cancellationToken);
        if (existing.Any())
        {
            return Result.Success();
        }

        var slots = new List<GroomingSlot>();
        var cursor = request.DayStart;
        var dayDate = request.Date.UtcDateTime.Date;

        while (cursor.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes)) <= request.DayEnd)
        {
            var end = cursor.Add(TimeSpan.FromMinutes(request.SlotDurationMinutes));
            slots.Add(new GroomingSlot(
                Guid.NewGuid(),
                request.GroomerId,
                new DateTimeOffset(dayDate, TimeSpan.Zero),
                cursor,
                end));
            cursor = end;
        }

        await _slotRepository.AddRangeAsync(slots, cancellationToken);
        return Result.Success();
    }
}

public sealed class BlockGroomingSlotCommandHandler : IRequestHandler<BlockGroomingSlotCommand, Result>
{
    private readonly IGroomingSlotRepository _slotRepository;

    public BlockGroomingSlotCommandHandler(IGroomingSlotRepository slotRepository) => _slotRepository = slotRepository;

    public async Task<Result> Handle(BlockGroomingSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
        if (slot is null)
        {
            return Result.Failure(ErrorCodes.GroomingSlot.NotFound);
        }

        var result = slot.Block();
        if (result.IsFailure)
        {
            return result;
        }

        _slotRepository.Update(slot);
        return Result.Success();
    }
}

public sealed class UnblockGroomingSlotCommandHandler : IRequestHandler<UnblockGroomingSlotCommand, Result>
{
    private readonly IGroomingSlotRepository _slotRepository;

    public UnblockGroomingSlotCommandHandler(IGroomingSlotRepository slotRepository) => _slotRepository = slotRepository;

    public async Task<Result> Handle(UnblockGroomingSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
        if (slot is null)
        {
            return Result.Failure(ErrorCodes.GroomingSlot.NotFound);
        }

        var result = slot.Unblock();
        if (result.IsFailure)
        {
            return result;
        }

        _slotRepository.Update(slot);
        return Result.Success();
    }
}
