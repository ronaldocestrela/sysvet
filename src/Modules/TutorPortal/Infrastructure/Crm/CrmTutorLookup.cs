using Core.Domain;
using Core.Domain.Entities;
using TutorPortal.Application.Abstractions;

namespace TutorPortal.Infrastructure.Crm;

/// <summary>
/// Reads CRM tutor and pet data through Core repositories for the tutor portal module.
/// </summary>
public sealed class CrmTutorLookup : ICrmTutorLookup
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;

    public CrmTutorLookup(ITutorRepository tutorRepository, IPetRepository petRepository)
    {
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
    }

    /// <inheritdoc />
    public async Task<CrmTutorMatch?> FindActiveTutorByEmailAndCpfAsync(
        string email,
        string cpf,
        CancellationToken cancellationToken = default)
    {
        var byEmail = await _tutorRepository.GetByEmailAsync(email.Trim(), cancellationToken);
        if (byEmail is null || !byEmail.IsActive)
        {
            return null;
        }

        var byCpf = await _tutorRepository.GetByCpfAsync(NormalizeCpf(cpf), cancellationToken);
        if (byCpf is null || !byCpf.IsActive)
        {
            return null;
        }

        if (byEmail.Id != byCpf.Id)
        {
            return null;
        }

        return Map(byEmail);
    }

    /// <inheritdoc />
    public async Task<CrmTutorMatch?> GetActiveTutorByIdAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutorRepository.GetByIdAsync(tutorId, cancellationToken);
        if (tutor is null || !tutor.IsActive)
        {
            return null;
        }

        return Map(tutor);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CrmPetSummary>> GetPetsForTutorAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var pets = await _petRepository.GetByTutorIdAsync(tutorId, cancellationToken);
        return pets
            .Where(p => !p.IsDeleted)
            .Select(p => new CrmPetSummary(p.Id, p.Name))
            .ToList();
    }

    private static CrmTutorMatch Map(Tutor tutor) =>
        new(tutor.Id, tutor.Name, tutor.Email.Address);

    private static string NormalizeCpf(string cpf) =>
        new string(cpf.Where(char.IsDigit).ToArray());
}
