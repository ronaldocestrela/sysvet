using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Petshop.Infrastructure.Persistence;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.PetHealth.Dtos;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.Services;
using Veterinary.Infrastructure.Persistence;

namespace TutorPortal.Infrastructure.PetHealth;

/// <summary>
/// Aggregates tutor-visible health data from Veterinary and Petshop schemas.
/// </summary>
public sealed class TutorPetHealthReadPort : ITutorPetHealthReadPort
{
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly IVaccineDoseRepository _doseRepository;
    private readonly IVaccineProtocolRepository _protocolRepository;
    private readonly IClinicalExamRepository _examRepository;
    private readonly VeterinaryDbContext _veterinaryDbContext;
    private readonly PetshopDbContext _petshopDbContext;

    /// <summary>
    /// Creates the read port with CRM and clinical dependencies.
    /// </summary>
    public TutorPetHealthReadPort(
        IPetRepository petRepository,
        ITutorRepository tutorRepository,
        IVaccineDoseRepository doseRepository,
        IVaccineProtocolRepository protocolRepository,
        IClinicalExamRepository examRepository,
        VeterinaryDbContext veterinaryDbContext,
        PetshopDbContext petshopDbContext)
    {
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
        _doseRepository = doseRepository;
        _protocolRepository = protocolRepository;
        _examRepository = examRepository;
        _veterinaryDbContext = veterinaryDbContext;
        _petshopDbContext = petshopDbContext;
    }

    /// <inheritdoc />
    public async Task<TutorVaccinationCardDto?> GetVaccinationCardAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
        if (pet is null || pet.IsDeleted)
        {
            return null;
        }

        var tutor = await _tutorRepository.GetByIdAsync(pet.TutorId, cancellationToken);
        var applied = await _doseRepository.GetByPetIdAsync(petId, cancellationToken);
        var appliedDoseIds = applied.Where(d => d.ProtocolDoseId.HasValue).Select(d => d.ProtocolDoseId!.Value).ToHashSet();

        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageInDays = VaccineSchedule.GetAgeInDays(pet.BirthDate, referenceDate);

        var protocols = await _protocolRepository.ListAsync(pet.Species, activeOnly: true, cancellationToken);
        var suggested = new List<TutorSuggestedVaccineDoseDto>();
        foreach (var protocol in protocols)
        {
            foreach (var dose in protocol.Doses.OrderBy(d => d.Sequence))
            {
                if (appliedDoseIds.Contains(dose.Id))
                {
                    continue;
                }

                if (!VaccineSchedule.IsAgeEligible(ageInDays, dose.MinAgeInDays, dose.MaxAgeInDays))
                {
                    continue;
                }

                suggested.Add(new TutorSuggestedVaccineDoseDto
                {
                    ProtocolName = protocol.Name,
                    Label = dose.Label,
                    Sequence = dose.Sequence
                });
            }
        }

        return new TutorVaccinationCardDto
        {
            PetId = pet.Id,
            PetName = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            TutorName = tutor?.Name ?? string.Empty,
            AppliedDoses = applied.Select(d => new TutorVaccineDoseDto
            {
                Id = d.Id,
                Name = d.Name,
                BatchNumber = d.BatchNumber,
                AppliedAt = d.AppliedAt,
                NextDueDate = d.NextDueDate
            }).ToList(),
            SuggestedDoses = suggested
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorPetExamDto>> ListExamsAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var exams = await _examRepository.GetByPetIdAsync(petId, cancellationToken);
        return exams.Select(e => new TutorPetExamDto
        {
            Id = e.Id,
            Name = e.Name,
            Category = e.Category.ToString(),
            Status = e.Status.ToString(),
            OccurredAt = e.UpdatedAt,
            ResultSummary = e.ResultSummary
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorPetTimelineItemDto>> ListTimelineAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var items = new List<TutorPetTimelineItemDto>();

        var appointments = await _veterinaryDbContext.Appointments.AsNoTracking()
            .Where(a => a.PetId == petId)
            .ToListAsync(cancellationToken);

        foreach (var appointment in appointments)
        {
            items.Add(new TutorPetTimelineItemDto
            {
                Id = appointment.Id,
                SourceType = "Appointment",
                OccurredAt = appointment.Date,
                Status = appointment.Status.ToString(),
                Title = string.IsNullOrWhiteSpace(appointment.Reason) ? "Consulta veterinária" : appointment.Reason.Trim()
            });
        }

        var groomingRows = await _petshopDbContext.GroomingAppointments.AsNoTracking()
            .Where(a => a.PetId == petId)
            .ToListAsync(cancellationToken);

        var serviceIds = groomingRows.Select(g => g.GroomingServiceId).Distinct().ToList();
        var services = await _petshopDbContext.GroomingServices.AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        foreach (var grooming in groomingRows)
        {
            var serviceName = services.TryGetValue(grooming.GroomingServiceId, out var name) ? name : "Banho e tosa";
            items.Add(new TutorPetTimelineItemDto
            {
                Id = grooming.Id,
                SourceType = "Grooming",
                OccurredAt = grooming.Date,
                Status = grooming.Status.ToString(),
                Title = serviceName
            });
        }

        items.Sort((a, b) => b.OccurredAt.CompareTo(a.OccurredAt));
        return items;
    }
}
