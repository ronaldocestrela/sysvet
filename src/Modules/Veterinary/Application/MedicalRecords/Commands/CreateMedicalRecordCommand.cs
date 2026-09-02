using Core.Domain;
using MediatR;
using System;

namespace Veterinary.Application.MedicalRecords.Commands;

public record CreateMedicalRecordCommand(Guid AppointmentId) : IRequest<Result<Guid>>;
