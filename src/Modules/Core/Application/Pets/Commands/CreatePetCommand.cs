using Core.Domain;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Entities;

namespace Core.Application.Pets.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record CreatePetCommand(string Name, PetSpecies Species, string Breed, PetSex Sex, Guid TutorId, Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
