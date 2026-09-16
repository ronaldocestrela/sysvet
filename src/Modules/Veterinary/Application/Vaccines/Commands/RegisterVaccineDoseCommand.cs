using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Vaccines.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record RegisterVaccineDoseCommand(
    Guid PetId,
    string Name,
    string BatchNumber,
    DateTimeOffset AppliedAt,
    DateTimeOffset? NextDueDate
) : ICommand<Guid>;
