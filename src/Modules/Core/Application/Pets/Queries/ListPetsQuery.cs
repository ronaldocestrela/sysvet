using Core.Domain;
using MediatR;

namespace Core.Application.Pets.Queries;

public record ListPetsQuery(int Page = 1, int PageSize = 10, Guid? TutorId = null) : IRequest<Result<IEnumerable<PetDto>>>;
