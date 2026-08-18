using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

public record UpdatePetCommand(Guid Id, string Name, PetSpecies Species, string Breed, PetSex Sex) : IRequest<Result>;
