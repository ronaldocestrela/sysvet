using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using Veterinary.Application.Vaccines.Dtos;

namespace Veterinary.Application.Vaccines.Commands;

/// <summary>Creates a tenant vaccine protocol.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.VaccinesWrite)]
public sealed record CreateVaccineProtocolCommand(
    string Name,
    PetSpecies Species,
    IReadOnlyList<VaccineProtocolDoseInput> Doses,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Updates protocol metadata and dose lines.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.VaccinesWrite)]
public sealed record UpdateVaccineProtocolCommand(
    Guid ProtocolId,
    string Name,
    PetSpecies Species,
    IReadOnlyList<VaccineProtocolDoseInput> Doses,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Deactivates a protocol.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.VaccinesWrite)]
public sealed record DeactivateVaccineProtocolCommand(Guid ProtocolId, Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Registers an applied vaccine dose for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.VaccinesWrite)]
public sealed record RegisterVaccineDoseCommand(
    Guid PetId,
    string Name,
    string BatchNumber,
    DateTimeOffset AppliedAt,
    DateTimeOffset? NextDueDate,
    Guid? ProtocolDoseId = null,
    Guid Id = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Lists vaccine protocols.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.VaccinesRead)]
public sealed record ListVaccineProtocolsQuery(PetSpecies? Species = null, bool ActiveOnly = true) : IQuery<IReadOnlyList<VaccineProtocolDto>>;

/// <summary>Lists applied doses for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.VaccinesRead)]
public sealed record ListVaccineDosesByPetQuery(Guid PetId) : IQuery<IReadOnlyList<VaccineDoseDto>>;

/// <summary>Builds the digital vaccination card for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.VaccinesRead)]
public sealed record GetVaccinationCardQuery(Guid PetId) : IQuery<VaccinationCardDto>;

/// <summary>Lists overdue or upcoming vaccine reminders.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.VaccinesRead)]
public sealed record ListVaccineAlertsQuery(
    VaccineAlertStatusDto? Status = null,
    int HorizonDays = 7,
    int Page = 1,
    int PageSize = 50) : IQuery<IReadOnlyList<VaccineAlertDto>>;
