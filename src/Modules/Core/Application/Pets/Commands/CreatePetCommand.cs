using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;

namespace Core.Application.Pets.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PetsWrite)]
public record CreatePetCommand(string Name, PetSpecies Species, string Breed, PetSex Sex, Guid TutorId, Guid Id = default, Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
