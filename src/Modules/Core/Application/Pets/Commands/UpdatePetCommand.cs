using Core.Domain;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Entities;

namespace Core.Application.Pets.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record UpdatePetCommand(Guid Id, string Name, PetSpecies Species, string Breed, PetSex Sex, Guid IdempotencyKey = default) : IIdempotentCommand;
