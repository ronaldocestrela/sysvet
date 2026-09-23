using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;

namespace Core.Application.Integration;

/// <summary>Resolves tutor names for intelligence ABC reports.</summary>
public sealed class GetTutorDisplayNamesRequestHandler
    : IRequestHandler<GetTutorDisplayNamesRequest, Result<IReadOnlyDictionary<Guid, string>>>
{
    private readonly ITutorRepository _tutorRepository;

    /// <summary>Creates the handler.</summary>
    public GetTutorDisplayNamesRequestHandler(ITutorRepository tutorRepository) => _tutorRepository = tutorRepository;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyDictionary<Guid, string>>> Handle(
        GetTutorDisplayNamesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TutorIds.Count == 0)
        {
            return Result.Success<IReadOnlyDictionary<Guid, string>>(new Dictionary<Guid, string>());
        }

        var distinct = request.TutorIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var map = new Dictionary<Guid, string>();
        foreach (var id in distinct)
        {
            var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
            if (tutor is not null)
            {
                map[id] = tutor.Name;
            }
        }

        return Result.Success<IReadOnlyDictionary<Guid, string>>(map);
    }
}
