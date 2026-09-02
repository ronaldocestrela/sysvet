using Core.Domain;
using MediatR;
using System;

namespace Veterinary.Application.Hospitalizations.Commands;

public record DischargePetCommand(Guid HospitalizationId) : IRequest<Result<bool>>;
