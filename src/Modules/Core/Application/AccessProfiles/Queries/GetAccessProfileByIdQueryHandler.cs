using Core.Application.AccessProfiles.Dtos;
using Core.Domain;
using MediatR;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Maps a profile aggregate to a DTO.
/// </summary>
public sealed class GetAccessProfileByIdQueryHandler : IRequestHandler<GetAccessProfileByIdQuery, Result<AccessProfileDto>>
{
    private readonly IAccessProfileRepository _accessProfileRepository;

    public GetAccessProfileByIdQueryHandler(IAccessProfileRepository accessProfileRepository)
    {
        _accessProfileRepository = accessProfileRepository;
    }

    /// <inheritdoc />
    public async Task<Result<AccessProfileDto>> Handle(GetAccessProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _accessProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<AccessProfileDto>(ErrorCodes.AccessProfile.NotFound);
        }

        return Result.Success(AccessProfileMappings.ToDto(profile));
    }
}
