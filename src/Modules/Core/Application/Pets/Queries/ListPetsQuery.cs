using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Pets.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record ListPetsQuery(int Page = 1, int PageSize = 10, Guid? TutorId = null) : IQuery<IEnumerable<PetDto>>;
