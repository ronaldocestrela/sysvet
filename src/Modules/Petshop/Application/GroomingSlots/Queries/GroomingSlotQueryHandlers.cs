using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingSlots.Queries;

public sealed class GetGroomingScheduleSlotsQueryHandler : IRequestHandler<GetGroomingScheduleSlotsQuery, Result<List<GroomingSlotDto>>>
{
    private readonly IGroomingSlotRepository _repository;

    public GetGroomingScheduleSlotsQueryHandler(IGroomingSlotRepository repository) => _repository = repository;

    public async Task<Result<List<GroomingSlotDto>>> Handle(GetGroomingScheduleSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _repository.GetAllSlotsForDayAsync(request.GroomerId, request.Date, cancellationToken);
        var dtos = slots
            .Select(s => new GroomingSlotDto(s.Id, s.GroomerId, s.Date, s.StartTime, s.EndTime, s.IsAvailable))
            .ToList();
        return Result.Success(dtos);
    }
}
