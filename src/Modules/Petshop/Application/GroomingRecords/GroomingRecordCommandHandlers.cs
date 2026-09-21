using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingRecords;

using ErrorCodes = Petshop.Domain.ErrorCodes;

public sealed class UpdateGroomingRecordCommandHandler : IRequestHandler<UpdateGroomingRecordCommand, Result>
{
    private readonly IGroomingRecordRepository _recordRepository;

    public UpdateGroomingRecordCommandHandler(IGroomingRecordRepository recordRepository) => _recordRepository = recordRepository;

    public async Task<Result> Handle(UpdateGroomingRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _recordRepository.GetByAppointmentIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.NotFound);
        }

        var notes = record.UpdateCoatNotes(request.CoatNotes);
        if (notes.IsFailure)
        {
            return notes;
        }

        var supplies = record.SetSupplyLines(request.SupplyLines.Select(l => (l.ProductId, l.Quantity)));
        if (supplies.IsFailure)
        {
            return supplies;
        }

        _recordRepository.Update(record);
        return Result.Success();
    }
}

public sealed class GetGroomingRecordByAppointmentQueryHandler : IRequestHandler<GetGroomingRecordByAppointmentQuery, Result<GroomingRecordDto>>
{
    private readonly IGroomingRecordRepository _recordRepository;

    public GetGroomingRecordByAppointmentQueryHandler(IGroomingRecordRepository recordRepository) => _recordRepository = recordRepository;

    public async Task<Result<GroomingRecordDto>> Handle(GetGroomingRecordByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var record = await _recordRepository.GetByAppointmentIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<GroomingRecordDto>(ErrorCodes.GroomingRecord.NotFound);
        }

        return Result.Success(Map(record));
    }

    internal static GroomingRecordDto Map(GroomingRecord record) =>
        new(
            record.Id,
            record.GroomingAppointmentId,
            record.PetId,
            record.CoatNotes,
            record.Status.ToString(),
            record.SupplyLines.Select(l => new GroomingRecordSupplyLineDto(l.ProductId, l.Quantity)).ToList());
}

public sealed class GetPetGroomingHistoryQueryHandler : IRequestHandler<GetPetGroomingHistoryQuery, Result<IReadOnlyList<GroomingHistoryItemDto>>>
{
    private readonly IGroomingRecordRepository _recordRepository;
    private readonly IGroomingAppointmentRepository _appointmentRepository;

    public GetPetGroomingHistoryQueryHandler(
        IGroomingRecordRepository recordRepository,
        IGroomingAppointmentRepository appointmentRepository)
    {
        _recordRepository = recordRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<IReadOnlyList<GroomingHistoryItemDto>>> Handle(GetPetGroomingHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await _recordRepository.ListByPetIdAsync(request.PetId, cancellationToken);
        var items = new List<GroomingHistoryItemDto>();

        foreach (var record in records.OrderByDescending(r => r.UpdatedAt))
        {
            var appointment = await _appointmentRepository.GetByIdAsync(record.GroomingAppointmentId, cancellationToken);
            if (appointment is null)
            {
                continue;
            }

            items.Add(new GroomingHistoryItemDto(
                appointment.Id,
                record.Id,
                appointment.Status == GroomingAppointmentStatus.Completed ? appointment.UpdatedAt : appointment.Date,
                record.CoatNotes,
                record.Status.ToString()));
        }

        return Result.Success<IReadOnlyList<GroomingHistoryItemDto>>(items);
    }
}
