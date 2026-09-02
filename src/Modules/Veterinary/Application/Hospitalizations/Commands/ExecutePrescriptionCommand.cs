using Core.Domain;
using MediatR;
using System;

namespace Veterinary.Application.Hospitalizations.Commands;

public record ExecutePrescriptionCommand(
    Guid HospitalizationId,
    string MedicationName,
    string Dose,
    string Notes,
    Guid ExecutedBy
) : IRequest<Result<Guid>>;
