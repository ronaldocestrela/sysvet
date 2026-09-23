using Core.Domain;
using Intelligence.Application.Dashboard.Dtos;
using Intelligence.Domain.Dashboard;
using Intelligence.Domain.Entities;
using Intelligence.Domain.Repositories;
using MediatR;
using DomainErrorCodes = Core.Domain.ErrorCodes;

namespace Intelligence.Application.Dashboard.Commands;

/// <summary>Persists validated widget layout for a profile.</summary>
public sealed class UpsertProfileDashboardLayoutCommandHandler : IRequestHandler<UpsertProfileDashboardLayoutCommand, Result>
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IProfileDashboardLayoutRepository _layoutRepository;
    private readonly IIntelligenceUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public UpsertProfileDashboardLayoutCommandHandler(
        IAccessProfileRepository accessProfileRepository,
        IProfileDashboardLayoutRepository layoutRepository,
        IIntelligenceUnitOfWork unitOfWork)
    {
        _accessProfileRepository = accessProfileRepository;
        _layoutRepository = layoutRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpsertProfileDashboardLayoutCommand request, CancellationToken cancellationToken)
    {
        var profile = await _accessProfileRepository.GetByIdAsync(request.AccessProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(DomainErrorCodes.AccessProfile.NotFound);
        }

        var slots = request.Slots.Select(s => new DashboardWidgetSlot
        {
            WidgetKey = s.WidgetKey,
            IsVisible = s.IsVisible,
            SortOrder = s.SortOrder
        }).ToList();

        var existing = await _layoutRepository.GetByAccessProfileIdAsync(request.AccessProfileId, cancellationToken);
        if (existing is null)
        {
            var create = ProfileDashboardLayout.Create(request.AccessProfileId, slots);
            if (create.IsFailure)
            {
                return Result.Failure(create.Error);
            }

            await _layoutRepository.AddAsync(create.Value, cancellationToken);
        }
        else
        {
            var update = existing.UpdateSlots(slots);
            if (update.IsFailure)
            {
                return update;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
