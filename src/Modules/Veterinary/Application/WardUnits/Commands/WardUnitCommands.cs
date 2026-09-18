using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Veterinary.Application.WardUnits.Dtos;

namespace Veterinary.Application.WardUnits.Commands;

/// <summary>Bed input for replace operations.</summary>
public sealed record WardBedInput(Guid? BedId, string Code, int SortOrder, bool IsActive);

/// <summary>Creates a ward unit with optional initial beds.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record CreateWardUnitCommand(string Name, IReadOnlyList<WardBedInput> Beds, Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Renames a ward unit.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record UpdateWardUnitCommand(Guid WardUnitId, string Name, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Replaces bed lines for a ward unit.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record ReplaceWardUnitBedsCommand(Guid WardUnitId, IReadOnlyList<WardBedInput> Beds, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Deactivates a ward unit.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record DeactivateWardUnitCommand(Guid WardUnitId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Lists ward units.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.HospitalizationsRead)]
public sealed record ListWardUnitsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<WardUnitListItemDto>>;

/// <summary>Gets ward unit detail.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.HospitalizationsRead)]
public sealed record GetWardUnitByIdQuery(Guid WardUnitId) : IQuery<WardUnitDetailDto>;
