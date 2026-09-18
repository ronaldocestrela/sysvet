using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.ScheduleSlots.DTOs;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.ScheduleSlots.Queries;

public class GetScheduleSlotsQueryHandler : IRequestHandler<GetScheduleSlotsQuery, Result<List<ScheduleSlotDto>>>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public GetScheduleSlotsQueryHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result<List<ScheduleSlotDto>>> Handle(GetScheduleSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _scheduleSlotRepository.GetAllSlotsForDayAsync(
            request.VeterinarianId,
            request.Date,
            cancellationToken);

        var dtos = slots.Select(s => new ScheduleSlotDto
        {
            Id = s.Id,
            VeterinarianId = s.VeterinarianId,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsAvailable = s.IsAvailable
        }).OrderBy(s => s.StartTime).ToList();

        return Result.Success(dtos);
    }
}
