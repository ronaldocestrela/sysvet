using Core.Domain;
using MediatR;
using System;

namespace Veterinary.Application.Hospitalizations.Commands;

public record AdmitPetCommand(Guid PetId, Guid VeterinarianId, string Reason) : IRequest<Result<Guid>>;
