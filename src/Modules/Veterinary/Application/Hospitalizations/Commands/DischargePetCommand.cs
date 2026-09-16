using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Hospitalizations.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record DischargePetCommand(Guid HospitalizationId) : ICommand<bool>;
