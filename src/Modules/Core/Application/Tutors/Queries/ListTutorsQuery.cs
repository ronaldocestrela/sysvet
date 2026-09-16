using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;

namespace Core.Application.Tutors.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record ListTutorsQuery(int Page = 1, int PageSize = 10, string? NameFilter = null, string? CpfFilter = null) : IQuery<PagedResult<TutorDto>>;
