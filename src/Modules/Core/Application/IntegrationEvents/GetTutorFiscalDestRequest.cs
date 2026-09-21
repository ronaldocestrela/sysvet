using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Loads tutor recipient data for NF-e (Core module).</summary>
public sealed class GetTutorFiscalDestRequest : IRequest<Result<TutorFiscalDest?>>
{
    public Guid TutorId { get; }

    public GetTutorFiscalDestRequest(Guid tutorId) => TutorId = tutorId;
}

/// <summary>Tutor as fiscal recipient.</summary>
public sealed class TutorFiscalDest
{
    public string Name { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Street { get; init; }
    public string? Number { get; init; }
    public string? Complement { get; init; }
    public string? District { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public int? IbgeCityCode { get; init; }
}
