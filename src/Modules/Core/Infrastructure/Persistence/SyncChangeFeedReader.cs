using Core.Application.Sync;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence;

/// <summary>
/// EF-backed change feed for sync pull, including soft-deleted tombstones.
/// </summary>
public sealed class SyncChangeFeedReader : ISyncChangeFeedReader
{
    private readonly CoreDbContext _dbContext;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    public SyncChangeFeedReader(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PullChangesResult> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        // SQLite provider in CI does not translate DateTimeOffset comparisons reliably; filter in memory after load.
        var tutors = await _dbContext.Tutors.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
        var tutorCandidates = tutors
            .Where(t => t.UpdatedAt > since)
            .OrderBy(t => t.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreTutors = tutorCandidates.Count > take;
        if (hasMoreTutors)
        {
            tutorCandidates = tutorCandidates.Take(take).ToList();
        }

        var pets = await _dbContext.Pets.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
        var petCandidates = pets
            .Where(p => p.UpdatedAt > since)
            .OrderBy(p => p.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMorePets = petCandidates.Count > take;
        if (hasMorePets)
        {
            petCandidates = petCandidates.Take(take).ToList();
        }

        var mappedTutors = tutorCandidates.Select(MapTutor).ToList();
        var mappedPets = petCandidates.Select(MapPet).ToList();

        var maxUpdated = since;
        foreach (var t in mappedTutors)
        {
            if (t.UpdatedAt > maxUpdated)
            {
                maxUpdated = t.UpdatedAt;
            }
        }

        foreach (var p in mappedPets)
        {
            if (p.UpdatedAt > maxUpdated)
            {
                maxUpdated = p.UpdatedAt;
            }
        }

        return new PullChangesResult
        {
            Tutors = mappedTutors,
            Pets = mappedPets,
            NextSince = maxUpdated,
            HasMore = hasMoreTutors || hasMorePets
        };
    }

    private static SyncTutorDto MapTutor(Tutor tutor) =>
        new()
        {
            Id = tutor.Id,
            Name = tutor.Name,
            Email = tutor.Email.Address,
            Cpf = tutor.Cpf.Number,
            Phone = tutor.Phone.Number,
            IsDeleted = tutor.IsDeleted,
            UpdatedAt = tutor.UpdatedAt,
            RowVersion = Convert.ToBase64String(tutor.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncPetDto MapPet(Pet pet) =>
        new()
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species.ToString(),
            Breed = pet.Breed,
            Sex = pet.Sex.ToString(),
            TutorId = pet.TutorId,
            IsDeleted = pet.IsDeleted,
            UpdatedAt = pet.UpdatedAt,
            RowVersion = Convert.ToBase64String(pet.RowVersion ?? Array.Empty<byte>())
        };
}
