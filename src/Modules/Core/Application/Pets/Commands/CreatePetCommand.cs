using Core.Domain;
using Core.Domain.Entities;
using MediatR;

using Core.Application.Common;

namespace Core.Application.Pets.Commands;

public record CreatePetCommand(string Name, PetSpecies Species, string Breed, PetSex Sex, Guid TutorId, Guid IdempotencyKey = default) : IIdempotentCommand<Result<Guid>>;
