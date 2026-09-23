using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain.Authorization;
using Core.Application.Entitlements;
using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.Entitlements;
using Intelligence.Application.Dashboard.Dtos;
using Intelligence.Domain;
using Intelligence.Domain.Dashboard;
using Intelligence.Domain.Repositories;
using MediatR;
using DomainErrorCodes = Core.Domain.ErrorCodes;

namespace Intelligence.Application.Dashboard.Queries;

/// <summary>Composes dashboard widgets from layout, entitlements and module KPI handlers.</summary>
public sealed class GetTenantDashboardQueryHandler : IRequestHandler<GetTenantDashboardQuery, Result<TenantDashboardDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IProfileDashboardLayoutRepository _layoutRepository;
    private readonly ITenantEntitlementReader _entitlementReader;
    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public GetTenantDashboardQueryHandler(
        ICurrentUser currentUser,
        IAccessProfileRepository accessProfileRepository,
        IProfileDashboardLayoutRepository layoutRepository,
        ITenantEntitlementReader entitlementReader,
        IMediator mediator)
    {
        _currentUser = currentUser;
        _accessProfileRepository = accessProfileRepository;
        _layoutRepository = layoutRepository;
        _entitlementReader = entitlementReader;
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<TenantDashboardDto>> Handle(GetTenantDashboardQuery request, CancellationToken cancellationToken)
    {
        var profile = await ResolveProfileAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure<TenantDashboardDto>(DomainErrorCodes.AccessProfile.NotFound);
        }

        var layout = await _layoutRepository.GetByAccessProfileIdAsync(profile.Id, cancellationToken);
        var visibleSlots = DashboardLayoutResolver.ResolveVisibleSlots(layout, profile.BaseRole);

        var (startUtc, endUtc, businessDate) = BusinessDayRange.ForInstant(DateTimeOffset.UtcNow);
        var tenantId = _currentUser.TenantId;
        var enabledModules = tenantId == Guid.Empty
            ? new HashSet<CommercialModule>(Enum.GetValues<CommercialModule>())
            : await _entitlementReader.GetEnabledModulesAsync(tenantId, cancellationToken);

        var widgets = new List<DashboardWidgetDto>(visibleSlots.Count);
        foreach (var slot in visibleSlots)
        {
            var module = WidgetModuleMapper.GetRequiredModule(slot.WidgetKey);
            if (module is not null && !enabledModules.Contains(module.Value))
            {
                continue;
            }

            widgets.Add(await LoadWidgetAsync(slot.WidgetKey, startUtc, endUtc, cancellationToken));
        }

        return Result.Success(new TenantDashboardDto
        {
            BusinessDate = businessDate,
            Widgets = widgets
        });
    }

    private async Task<DashboardWidgetDto> LoadWidgetAsync(
        string widgetKey,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            return await LoadWidgetCoreAsync(widgetKey, startUtc, endUtc, cancellationToken);
        }
        catch (Exception)
        {
            return new DashboardWidgetDto
            {
                Key = widgetKey,
                IsSuccess = false,
                ErrorCode = "Intelligence.Widget.LoadFailed"
            };
        }
    }

    private async Task<DashboardWidgetDto> LoadWidgetCoreAsync(
        string widgetKey,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        Result<object> payloadResult = widgetKey switch
        {
            WidgetCatalog.SalesToday => await MapPayloadAsync(
                _mediator.Send(new GetSalesTodayKpisRequest(startUtc, endUtc), cancellationToken)),
            WidgetCatalog.SalesByHour => await MapPayloadAsync(
                _mediator.Send(new GetSalesByHourKpisRequest(startUtc, endUtc, BusinessDayRange.TimeZoneId), cancellationToken)),
            WidgetCatalog.GroomingToday => await MapPayloadAsync(
                _mediator.Send(new GetGroomingTodayKpisRequest(startUtc, endUtc), cancellationToken)),
            WidgetCatalog.ClinicalAppointmentsToday => await MapPayloadAsync(
                _mediator.Send(new GetClinicalAppointmentsTodayKpisRequest(startUtc, endUtc), cancellationToken)),
            WidgetCatalog.OnlineOrdersToday => await MapPayloadAsync(
                _mediator.Send(new GetOnlineOrdersTodayKpisRequest(startUtc, endUtc), cancellationToken)),
            _ => Result.Failure<object>(Intelligence.Domain.ErrorCodes.DashboardLayout.UnknownWidget)
        };

        if (payloadResult.IsFailure)
        {
            return new DashboardWidgetDto
            {
                Key = widgetKey,
                IsSuccess = false,
                ErrorCode = payloadResult.Error.Code
            };
        }

        return new DashboardWidgetDto
        {
            Key = widgetKey,
            IsSuccess = true,
            Data = payloadResult.Value
        };
    }

    private static async Task<Result<object>> MapPayloadAsync<T>(Task<Result<T>> task)
    {
        var result = await task;
        return result.IsSuccess
            ? Result.Success<object>(result.Value!)
            : Result.Failure<object>(result.Error);
    }

    private async Task<AccessProfile?> ResolveProfileAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.AccessProfileId != Guid.Empty)
        {
            var byId = await _accessProfileRepository.GetByIdAsync(_currentUser.AccessProfileId, cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        var role = _currentUser.Roles.FirstOrDefault();
        if (role is null)
        {
            return null;
        }

        var systemProfile = await _accessProfileRepository.GetSystemProfileByBaseRoleForTenantAsync(
            role,
            _currentUser.TenantId,
            cancellationToken);
        if (systemProfile is not null)
        {
            return systemProfile;
        }

        return role switch
        {
            ApplicationRoles.Admin => AccessProfile.CreateSystem(ApplicationRoles.Admin, ApplicationRoles.Admin, Permissions.AdminDefaults()).Value,
            ApplicationRoles.Veterinarian => AccessProfile.CreateSystem(ApplicationRoles.Veterinarian, ApplicationRoles.Veterinarian, Permissions.VeterinarianDefaults()).Value,
            ApplicationRoles.Receptionist => AccessProfile.CreateSystem(ApplicationRoles.Receptionist, ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults()).Value,
            ApplicationRoles.Cashier => AccessProfile.CreateSystem(ApplicationRoles.Cashier, ApplicationRoles.Cashier, Permissions.CashierDefaults()).Value,
            _ => null
        };
    }
}
