using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Petshop.Application.GroomingRecords;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record UpdateGroomingRecordCommand(
    Guid GroomingAppointmentId,
    string CoatNotes,
    IReadOnlyList<GroomingRecordSupplyLineDto> SupplyLines,
    Guid IdempotencyKey = default) : IIdempotentCommand;

public sealed record GroomingRecordSupplyLineDto(Guid ProductId, decimal Quantity);

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record GetGroomingRecordByAppointmentQuery(Guid GroomingAppointmentId) : IQuery<GroomingRecordDto>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record GetPetGroomingHistoryQuery(Guid PetId) : IQuery<IReadOnlyList<GroomingHistoryItemDto>>;

public sealed record GroomingRecordDto(
    Guid Id,
    Guid GroomingAppointmentId,
    Guid PetId,
    string CoatNotes,
    string Status,
    IReadOnlyList<GroomingRecordSupplyLineDto> SupplyLines);

public sealed record GroomingHistoryItemDto(
    Guid GroomingAppointmentId,
    Guid GroomingRecordId,
    DateTimeOffset CompletedAt,
    string CoatNotes,
    string Status);
