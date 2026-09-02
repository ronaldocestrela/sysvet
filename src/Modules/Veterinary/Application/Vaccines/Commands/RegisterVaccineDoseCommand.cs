using Core.Domain;
using MediatR;
using System;

namespace Veterinary.Application.Vaccines.Commands;

public record RegisterVaccineDoseCommand(
    Guid PetId,
    string Name,
    string BatchNumber,
    DateTimeOffset AppliedAt,
    DateTimeOffset? NextDueDate
) : IRequest<Result<Guid>>;
