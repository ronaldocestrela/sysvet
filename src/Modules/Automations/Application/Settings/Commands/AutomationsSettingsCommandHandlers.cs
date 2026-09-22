using Automations.Application.Settings.Dtos;
using Automations.Domain.Entities;
using Automations.Domain.Repositories;
using Automations.Domain.ValueObjects;
using Core.Domain;
using MediatR;

namespace Automations.Application.Settings.Commands;

/// <summary>
/// Handles Automations settings and tutor preference commands/queries.
/// </summary>
public sealed class GetAutomationsSettingsQueryHandler : IRequestHandler<GetAutomationsSettingsQuery, Result<AutomationsSettingsDto>>
{
    private readonly IAutomationsSettingsRepository _repository;

    public GetAutomationsSettingsQueryHandler(IAutomationsSettingsRepository repository) => _repository = repository;

    public async Task<Result<AutomationsSettingsDto>> Handle(GetAutomationsSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetSingletonAsync(cancellationToken);
        if (settings is null)
        {
            var defaults = AutomationsSettings.CreateDefault().Value;
            return Result.Success(Map(defaults));
        }

        return Result.Success(Map(settings));
    }

    private static AutomationsSettingsDto Map(AutomationsSettings settings)
    {
        var hours = settings.ToBusinessHours();
        var days = hours.IsSuccess
            ? hours.Value.Days.Select(d => d.ToString()).ToList()
            : new List<string>();

        return new AutomationsSettingsDto
        {
            TimeZoneId = settings.TimeZoneId,
            BusinessStart = settings.BusinessStart.ToString("HH:mm"),
            BusinessEnd = settings.BusinessEnd.ToString("HH:mm"),
            BusinessDays = days
        };
    }
}

public sealed class UpdateAutomationsSettingsCommandHandler : IRequestHandler<UpdateAutomationsSettingsCommand, Result>
{
    private readonly IAutomationsSettingsRepository _repository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public UpdateAutomationsSettingsCommandHandler(
        IAutomationsSettingsRepository repository,
        IAutomationsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAutomationsSettingsCommand request, CancellationToken cancellationToken)
    {
        var start = TimeOnly.Parse(request.BusinessStart);
        var end = TimeOnly.Parse(request.BusinessEnd);
        var days = request.BusinessDays.Select(Enum.Parse<DayOfWeek>).ToList();
        var hours = BusinessHours.Create(start, end, days);
        if (hours.IsFailure)
        {
            return Result.Failure(hours.Error);
        }

        var settings = await _repository.GetSingletonAsync(cancellationToken);
        if (settings is null)
        {
            var created = AutomationsSettings.CreateDefault();
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            settings = created.Value;
            var update = settings.Update(request.TimeZoneId, hours.Value);
            if (update.IsFailure)
            {
                return update;
            }

            _repository.Add(settings);
        }
        else
        {
            var update = settings.Update(request.TimeZoneId, hours.Value);
            if (update.IsFailure)
            {
                return update;
            }

            _repository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class GetTutorMessagingPreferenceQueryHandler : IRequestHandler<GetTutorMessagingPreferenceQuery, Result<TutorMessagingPreferenceDto>>
{
    private readonly ITutorMessagingPreferenceRepository _repository;

    public GetTutorMessagingPreferenceQueryHandler(ITutorMessagingPreferenceRepository repository) =>
        _repository = repository;

    public async Task<Result<TutorMessagingPreferenceDto>> Handle(
        GetTutorMessagingPreferenceQuery request,
        CancellationToken cancellationToken)
    {
        var pref = await _repository.GetByTutorIdAsync(request.TutorId, cancellationToken)
                   ?? TutorMessagingPreference.DefaultFor(request.TutorId);

        return Result.Success(new TutorMessagingPreferenceDto
        {
            TutorId = pref.TutorId,
            WhatsAppEnabled = pref.WhatsAppEnabled,
            EmailEnabled = pref.EmailEnabled,
            MarketingEnabled = pref.MarketingEnabled
        });
    }
}

public sealed class UpdateTutorMessagingPreferenceCommandHandler : IRequestHandler<UpdateTutorMessagingPreferenceCommand, Result>
{
    private readonly ITutorMessagingPreferenceRepository _repository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public UpdateTutorMessagingPreferenceCommandHandler(
        ITutorMessagingPreferenceRepository repository,
        IAutomationsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTutorMessagingPreferenceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByTutorIdAsync(request.TutorId, cancellationToken);
        if (existing is null)
        {
            var created = TutorMessagingPreference.Create(
                request.TutorId,
                request.WhatsAppEnabled,
                request.EmailEnabled,
                request.MarketingEnabled);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _repository.Add(created.Value);
        }
        else
        {
            var update = existing.Update(request.WhatsAppEnabled, request.EmailEnabled, request.MarketingEnabled);
            if (update.IsFailure)
            {
                return update;
            }

            _repository.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
