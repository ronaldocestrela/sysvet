using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Hospitalizations.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record ExecutePrescriptionCommand(
    Guid HospitalizationId,
    string MedicationName,
    string Dose,
    string Notes,
    Guid ExecutedBy
) : ICommand<Guid>;
