using Clients.Infrastructure.Http;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Offline adapter: persists tutors in local SQLite and enqueues outbox messages on save.
/// </summary>
public sealed class OfflineTutorStore : ITutorStore
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;
    private readonly OfflineDbContext _dbContext;

    /// <summary>Creates the store.</summary>
    public OfflineTutorStore(ITutorRepository tutorRepository, IPetRepository petRepository, OfflineDbContext dbContext)
    {
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResultDto<TutorDto>>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var pageResult = await _tutorRepository.SearchAsync(page, pageSize, null, null, cancellationToken);
        var dto = new PagedResultDto<TutorDto>
        {
            Items = pageResult.Items.Select(CrmDtoMappings.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = pageResult.TotalCount
        };
        return Result.Success(dto);
    }

    /// <inheritdoc />
    public async Task<Result<TutorDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        return tutor is null
            ? Result.Failure<TutorDto>(ErrorCodes.Tutor.NotFound)
            : Result.Success(CrmDtoMappings.ToDto(tutor));
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateAsync(CreateTutorRequest request, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Guid>(emailResult.Error);
        }

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
        {
            return Result.Failure<Guid>(cpfResult.Error);
        }

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure)
        {
            return Result.Failure<Guid>(phoneResult.Error);
        }

        var id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
        var tutorResult = Tutor.Create(request.Name, emailResult.Value, cpfResult.Value, phoneResult.Value, id);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<Guid>(tutorResult.Error);
        }

        _tutorRepository.Add(tutorResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(tutorResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> UpdateAsync(UpdateTutorRequest request, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure)
        {
            return Result.Failure(phoneResult.Error);
        }

        var updateResult = tutor.Update(request.Name, emailResult.Value, phoneResult.Value);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _tutorRepository.Update(tutor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        tutor.SoftDelete();
        var pets = await _petRepository.GetByTutorIdAsync(id, cancellationToken);
        foreach (var pet in pets)
        {
            pet.SoftDelete();
            _petRepository.Update(pet);
        }

        _tutorRepository.Update(tutor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
