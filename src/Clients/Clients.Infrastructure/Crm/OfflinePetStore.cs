using Clients.Infrastructure.Http;
using Core.Domain;
using Core.Domain.Entities;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Offline adapter: persists pets in local SQLite and enqueues outbox messages on save.
/// </summary>
public sealed class OfflinePetStore : IPetStore
{
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly OfflineDbContext _dbContext;

    /// <summary>Creates the store.</summary>
    public OfflinePetStore(IPetRepository petRepository, ITutorRepository tutorRepository, OfflineDbContext dbContext)
    {
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResultDto<PetDto>>> ListAsync(int page, int pageSize, Guid? tutorId = null, CancellationToken cancellationToken = default)
    {
        var pageResult = await _petRepository.SearchAsync(page, pageSize, tutorId, null, cancellationToken);
        var dto = new PagedResultDto<PetDto>
        {
            Items = pageResult.Items.Select(CrmDtoMappings.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = pageResult.TotalCount
        };
        return Result.Success(dto);
    }

    /// <inheritdoc />
    public async Task<Result<PetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
        return pet is null
            ? Result.Failure<PetDto>(ErrorCodes.Pet.NotFound)
            : Result.Success(CrmDtoMappings.ToDto(pet));
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor is null || !tutor.IsActive)
        {
            return Result.Failure<Guid>(ErrorCodes.Pet.TutorInactive);
        }

        var id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
        var petResult = Pet.Create(
            request.Name,
            CrmDtoMappings.ToDomainSpecies(request.Species),
            request.Breed,
            CrmDtoMappings.ToDomainSex(request.Sex),
            request.TutorId,
            id);

        if (petResult.IsFailure)
        {
            return Result.Failure<Guid>(petResult.Error);
        }

        _petRepository.Add(petResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(petResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> UpdateAsync(UpdatePetRequest request, CancellationToken cancellationToken = default)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet is null)
        {
            return Result.Failure(ErrorCodes.Pet.NotFound);
        }

        var updateResult = pet.Update(
            request.Name,
            CrmDtoMappings.ToDomainSpecies(request.Species),
            request.Breed,
            CrmDtoMappings.ToDomainSex(request.Sex));

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _petRepository.Update(pet);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
        if (pet is null)
        {
            return Result.Failure(ErrorCodes.Pet.NotFound);
        }

        pet.SoftDelete();
        _petRepository.Update(pet);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
