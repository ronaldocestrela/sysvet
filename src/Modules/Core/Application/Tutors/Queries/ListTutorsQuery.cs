using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Queries;

public record ListTutorsQuery(int Page = 1, int PageSize = 10, string? NameFilter = null, string? CpfFilter = null) : IRequest<Result<IEnumerable<TutorDto>>>;
