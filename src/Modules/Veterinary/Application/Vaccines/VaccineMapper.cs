using Veterinary.Application.Vaccines.Dtos;
using Veterinary.Domain.Entities;

namespace Veterinary.Application.Vaccines;

/// <summary>Maps vaccine domain entities to application DTOs.</summary>
public static class VaccineMapper
{
    public static VaccineDoseDto ToDto(VaccineDose dose) => new()
    {
        Id = dose.Id,
        PetId = dose.PetId,
        Name = dose.Name,
        BatchNumber = dose.BatchNumber,
        AppliedAt = dose.AppliedAt,
        NextDueDate = dose.NextDueDate,
        ProtocolId = dose.ProtocolId,
        ProtocolDoseId = dose.ProtocolDoseId
    };

    public static VaccineProtocolDto ToDto(VaccineProtocol protocol) => new()
    {
        Id = protocol.Id,
        Name = protocol.Name,
        Species = protocol.Species,
        IsActive = protocol.IsActive,
        Doses = protocol.Doses
            .OrderBy(d => d.Sequence)
            .Select(d => new VaccineProtocolDoseDto
            {
                Id = d.Id,
                Sequence = d.Sequence,
                Label = d.Label,
                MinAgeInDays = d.MinAgeInDays,
                MaxAgeInDays = d.MaxAgeInDays,
                IntervalFromPreviousInDays = d.IntervalFromPreviousInDays,
                NextDoseIntervalInDays = d.NextDoseIntervalInDays
            })
            .ToList()
    };
}
