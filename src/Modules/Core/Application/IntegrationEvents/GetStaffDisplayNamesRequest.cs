using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Resolves staff display names by user id for intelligence reports (Identity).</summary>
public sealed class GetStaffDisplayNamesRequest : IRequest<Result<IReadOnlyDictionary<Guid, string>>>
{
    /// <summary>Staff user ids (Guid form used in Sales/Veterinary/Petshop).</summary>
    public IReadOnlyList<Guid> UserIds { get; }

    /// <summary>Creates the request.</summary>
    public GetStaffDisplayNamesRequest(IReadOnlyList<Guid> userIds) => UserIds = userIds;
}
