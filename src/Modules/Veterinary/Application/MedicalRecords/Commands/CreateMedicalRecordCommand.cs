using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.MedicalRecords.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record CreateMedicalRecordCommand(Guid AppointmentId) : ICommand<Guid>;
