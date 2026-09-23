using Core.Domain;
using Intelligence.Application.Dashboard;
using Intelligence.Application.Dashboard.Dtos;
using Intelligence.Domain.Repositories;
using MediatR;
using DomainErrorCodes = Core.Domain.ErrorCodes;

namespace Intelligence.Application.Dashboard.Queries;

/// <summary>Maps stored or default layout for an access profile.</summary>
public sealed class GetProfileDashboardLayoutQueryHandler
    : IRequestHandler<GetProfileDashboardLayoutQuery, Result<ProfileDashboardLayoutDto>>
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IProfileDashboardLayoutRepository _layoutRepository;

    /// <summary>Creates the handler.</summary>
    public GetProfileDashboardLayoutQueryHandler(
        IAccessProfileRepository accessProfileRepository,
        IProfileDashboardLayoutRepository layoutRepository)
    {
        _accessProfileRepository = accessProfileRepository;
        _layoutRepository = layoutRepository;
    }

    /// <inheritdoc />
    public async Task<Result<ProfileDashboardLayoutDto>> Handle(
        GetProfileDashboardLayoutQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _accessProfileRepository.GetByIdAsync(request.AccessProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<ProfileDashboardLayoutDto>(DomainErrorCodes.AccessProfile.NotFound);
        }

        var layout = await _layoutRepository.GetByAccessProfileIdAsync(request.AccessProfileId, cancellationToken);
        var slots = layout?.Slots ?? DefaultDashboardLayout.ForBaseRole(profile.BaseRole);

        return Result.Success(new ProfileDashboardLayoutDto
        {
            AccessProfileId = request.AccessProfileId,
            Slots = slots.Select(s => new DashboardWidgetSlotDto
            {
                WidgetKey = s.WidgetKey,
                IsVisible = s.IsVisible,
                SortOrder = s.SortOrder
            }).ToList()
        });
    }
}
