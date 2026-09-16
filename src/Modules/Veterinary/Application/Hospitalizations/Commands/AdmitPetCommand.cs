using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Hospitalizations.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record AdmitPetCommand(Guid PetId, Guid VeterinarianId, string Reason) : ICommand<Guid>;
