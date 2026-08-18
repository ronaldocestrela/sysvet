using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

public record CreatePetCommand(string Name, PetSpecies Species, string Breed, PetSex Sex, Guid TutorId) : IRequest<Result<Guid>>;
