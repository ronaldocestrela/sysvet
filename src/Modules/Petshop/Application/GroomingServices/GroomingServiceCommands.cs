using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Petshop.Domain.Enums;

namespace Petshop.Application.GroomingServices;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record UpsertGroomingServiceCommand(
    Guid Id,
    string Name,
    GroomingServiceType ServiceType,
    int DurationInMinutes,
    string? PrepaidServiceCode,
    bool IsActive,
    IReadOnlyList<GroomingServiceSupplyLineDto> DefaultSupplies,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

public sealed record GroomingServiceSupplyLineDto(Guid ProductId, decimal Quantity);

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record ListGroomingServicesQuery : IQuery<IReadOnlyList<GroomingServiceDto>>;

public sealed record GroomingServiceDto(
    Guid Id,
    string Name,
    GroomingServiceType ServiceType,
    int DurationInMinutes,
    string? PrepaidServiceCode,
    bool IsActive,
    IReadOnlyList<GroomingServiceSupplyLineDto> DefaultSupplies);
