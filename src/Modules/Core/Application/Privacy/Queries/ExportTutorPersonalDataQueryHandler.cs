using Core.Application.Tutors;
using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Privacy.Queries;

/// <summary>Builds a tutor personal-data export from CRM and module contributors.</summary>
public sealed class ExportTutorPersonalDataQueryHandler : IRequestHandler<ExportTutorPersonalDataQuery, Result<TutorPersonalDataExportDto>>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;
    private readonly IEnumerable<IPersonalDataExportContributor> _exportContributors;

    /// <summary>Initializes handler dependencies.</summary>
    public ExportTutorPersonalDataQueryHandler(
        ITutorRepository tutorRepository,
        IPetRepository petRepository,
        IEnumerable<IPersonalDataExportContributor> exportContributors)
    {
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
        _exportContributors = exportContributors;
    }

    /// <inheritdoc />
    public async Task<Result<TutorPersonalDataExportDto>> Handle(
        ExportTutorPersonalDataQuery request,
        CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure<TutorPersonalDataExportDto>(ErrorCodes.Tutor.NotFound);
        }

        var pets = await _petRepository.GetByTutorIdAsync(request.TutorId, cancellationToken);
        var modules = new Dictionary<string, Dictionary<string, object?>>();

        foreach (var contributor in _exportContributors)
        {
            var slice = await contributor.GetSlicesAsync(request.TutorId, cancellationToken);
            modules[contributor.ModuleKey] = slice.ToDictionary(x => x.Key, x => x.Value);
        }

        var dto = new TutorPersonalDataExportDto
        {
            TutorId = tutor.Id,
            ExportedAt = DateTimeOffset.UtcNow,
            Tutor = TutorMappings.ToDto(tutor),
            Pets = pets.Select(MapPet).ToList(),
            Modules = modules
        };

        return Result.Success(dto);
    }

    private static PrivacyPetDto MapPet(Pet pet) => new()
    {
        Id = pet.Id,
        Name = pet.Name,
        Species = pet.Species.ToString()
    };
}
