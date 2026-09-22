using ClinicSite.Application.Dtos;
using ClinicSite.Domain.Entities;
using ClinicSiteError = ClinicSite.Domain.ErrorCodes;
using ClinicSite.Domain.Repositories;
using ClinicSite.Domain.ValueObjects;
using Core.Domain;
using MediatR;

namespace ClinicSite.Application.Commands;

/// <summary>
/// Loads or initializes the tenant clinic site profile.
/// </summary>
public sealed class GetClinicSiteQueryHandler : IRequestHandler<GetClinicSiteQuery, Result<ClinicSiteStaffDto>>
{
    private readonly IClinicSiteProfileRepository _profiles;

    public GetClinicSiteQueryHandler(IClinicSiteProfileRepository profiles) => _profiles = profiles;

    public async Task<Result<ClinicSiteStaffDto>> Handle(GetClinicSiteQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            profile = ClinicSiteProfile.CreateDefault().Value;
        }

        return Result.Success(ClinicSiteMapping.ToStaffDto(profile));
    }
}

/// <summary>
/// Updates profile fields and slug reservation.
/// </summary>
public sealed class UpdateClinicSiteProfileCommandHandler : IRequestHandler<UpdateClinicSiteProfileCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteSlugIndexRepository _slugIndex;
    private readonly IClinicSiteUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateClinicSiteProfileCommandHandler(
        IClinicSiteProfileRepository profiles,
        IClinicSiteSlugIndexRepository slugIndex,
        IClinicSiteUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _profiles = profiles;
        _slugIndex = slugIndex;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateClinicSiteProfileCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId == Guid.Empty)
        {
            return Result.Failure(ClinicSiteError.Site.NotFound);
        }

        var profile = await _profiles.GetAsync(cancellationToken);
        var isNew = profile is null;
        profile ??= ClinicSiteProfile.CreateDefault().Value;

        var update = profile.UpdateProfile(
            request.DisplayName,
            request.Tagline,
            request.Street,
            request.Number,
            request.Complement,
            request.District,
            request.City,
            request.State,
            request.PostalCode,
            request.Phone,
            request.Email,
            request.WhatsApp,
            request.LogoUrl);
        if (update.IsFailure)
        {
            return update;
        }

        var slugResult = await ReserveSlugAsync(profile, request.Slug, cancellationToken);
        if (slugResult.IsFailure)
        {
            return slugResult;
        }

        if (isNew)
        {
            await _profiles.AddAsync(profile, cancellationToken);
        }
        else
        {
            await _profiles.UpdateAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ReserveSlugAsync(ClinicSiteProfile profile, string slug, CancellationToken cancellationToken)
    {
        var normalized = PublicSiteSlug.Normalize(slug);
        if (string.IsNullOrEmpty(normalized))
        {
            profile.SetSlug(string.Empty);
            return Result.Success();
        }

        if (!PublicSiteSlug.IsValid(normalized))
        {
            return Result.Failure(ClinicSiteError.Site.InvalidSlug);
        }

        var existingForTenant = await _slugIndex.GetByTenantIdAsync(_tenantContext.TenantId, cancellationToken);
        var taken = await _slugIndex.GetBySlugAsync(normalized, cancellationToken);
        if (taken is not null && taken.TenantId != _tenantContext.TenantId)
        {
            return Result.Failure(ClinicSiteError.Site.SlugTaken);
        }

        if (existingForTenant is null)
        {
            var created = ClinicSiteSlugIndex.Create(normalized, _tenantContext.TenantId, profile.IsPublished);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            await _slugIndex.AddAsync(created.Value, cancellationToken);
        }
        else
        {
            var updateSlug = existingForTenant.UpdateSlug(normalized);
            if (updateSlug.IsFailure)
            {
                return updateSlug;
            }

            if (profile.IsPublished)
            {
                existingForTenant.MarkPublished();
            }

            await _slugIndex.UpdateAsync(existingForTenant, cancellationToken);
        }

        return profile.SetSlug(normalized);
    }
}

/// <summary>
/// Replaces public service lines.
/// </summary>
public sealed class ReplaceClinicSiteServicesCommandHandler : IRequestHandler<ReplaceClinicSiteServicesCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteUnitOfWork _unitOfWork;

    public ReplaceClinicSiteServicesCommandHandler(IClinicSiteProfileRepository profiles, IClinicSiteUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReplaceClinicSiteServicesCommand request, CancellationToken cancellationToken)
    {
        var profile = await EnsureProfileAsync(cancellationToken);
        if (profile.IsFailure)
        {
            return Result.Failure(profile.Error);
        }

        var lines = request.Services.Select(s => (s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.SortOrder, s.IsVisible));
        var replace = profile.Value.ReplaceServices(lines);
        if (replace.IsFailure)
        {
            return replace;
        }

        await _profiles.UpdateAsync(profile.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<ClinicSiteProfile>> EnsureProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure<ClinicSiteProfile>(ClinicSiteError.Site.NotFound);
        }

        return Result.Success(profile);
    }
}

/// <summary>
/// Replaces public team members.
/// </summary>
public sealed class ReplaceClinicSiteTeamCommandHandler : IRequestHandler<ReplaceClinicSiteTeamCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteUnitOfWork _unitOfWork;

    public ReplaceClinicSiteTeamCommandHandler(IClinicSiteProfileRepository profiles, IClinicSiteUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReplaceClinicSiteTeamCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ClinicSiteError.Site.NotFound);
        }

        var members = request.Team.Select(t => (t.Id, t.Name, t.RoleTitle, t.Bio, t.SortOrder, t.IsVisible));
        var replace = profile.ReplaceTeam(members);
        if (replace.IsFailure)
        {
            return replace;
        }

        await _profiles.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Replaces weekly opening hours.
/// </summary>
public sealed class ReplaceClinicSiteHoursCommandHandler : IRequestHandler<ReplaceClinicSiteHoursCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteUnitOfWork _unitOfWork;

    public ReplaceClinicSiteHoursCommandHandler(IClinicSiteProfileRepository profiles, IClinicSiteUnitOfWork unitOfWork)
    {
        _profiles = profiles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReplaceClinicSiteHoursCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ClinicSiteError.Site.NotFound);
        }

        var rows = request.Hours.Select(h =>
        {
            TimeOnly? open = string.IsNullOrWhiteSpace(h.OpenTime) ? null : TimeOnly.Parse(h.OpenTime);
            TimeOnly? close = string.IsNullOrWhiteSpace(h.CloseTime) ? null : TimeOnly.Parse(h.CloseTime);
            return (h.Id, h.Day, open, close, h.IsClosed);
        });

        var replace = profile.ReplaceHours(rows);
        if (replace.IsFailure)
        {
            return replace;
        }

        await _profiles.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Publishes the site and slug index.
/// </summary>
public sealed class PublishClinicSiteCommandHandler : IRequestHandler<PublishClinicSiteCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteSlugIndexRepository _slugIndex;
    private readonly IClinicSiteUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PublishClinicSiteCommandHandler(
        IClinicSiteProfileRepository profiles,
        IClinicSiteSlugIndexRepository slugIndex,
        IClinicSiteUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _profiles = profiles;
        _slugIndex = slugIndex;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(PublishClinicSiteCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ClinicSiteError.Site.NotFound);
        }

        var publish = profile.Publish();
        if (publish.IsFailure)
        {
            return publish;
        }

        var index = await _slugIndex.GetByTenantIdAsync(_tenantContext.TenantId, cancellationToken);
        if (index is null || !string.Equals(index.Slug, profile.Slug, StringComparison.Ordinal))
        {
            return Result.Failure(ClinicSiteError.Site.InvalidSlug);
        }

        var taken = await _slugIndex.GetBySlugAsync(profile.Slug, cancellationToken);
        if (taken is not null && taken.TenantId != _tenantContext.TenantId)
        {
            return Result.Failure(ClinicSiteError.Site.SlugTaken);
        }

        index.MarkPublished();
        await _slugIndex.UpdateAsync(index, cancellationToken);
        await _profiles.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Unpublishes the site.
/// </summary>
public sealed class UnpublishClinicSiteCommandHandler : IRequestHandler<UnpublishClinicSiteCommand, Result>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteSlugIndexRepository _slugIndex;
    private readonly IClinicSiteUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UnpublishClinicSiteCommandHandler(
        IClinicSiteProfileRepository profiles,
        IClinicSiteSlugIndexRepository slugIndex,
        IClinicSiteUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _profiles = profiles;
        _slugIndex = slugIndex;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UnpublishClinicSiteCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ClinicSiteError.Site.NotFound);
        }

        profile.Unpublish();
        var index = await _slugIndex.GetByTenantIdAsync(_tenantContext.TenantId, cancellationToken);
        index?.MarkUnpublished();
        if (index is not null)
        {
            await _slugIndex.UpdateAsync(index, cancellationToken);
        }

        await _profiles.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>
/// Returns published site content for anonymous visitors.
/// </summary>
public sealed class GetPublicClinicSiteQueryHandler : IRequestHandler<GetPublicClinicSiteQuery, Result<PublicClinicSiteDto>>
{
    private readonly IClinicSiteProfileRepository _profiles;
    private readonly IClinicSiteSlugIndexRepository _slugIndex;

    public GetPublicClinicSiteQueryHandler(
        IClinicSiteProfileRepository profiles,
        IClinicSiteSlugIndexRepository slugIndex)
    {
        _profiles = profiles;
        _slugIndex = slugIndex;
    }

    public async Task<Result<PublicClinicSiteDto>> Handle(GetPublicClinicSiteQuery request, CancellationToken cancellationToken)
    {
        var normalized = PublicSiteSlug.Normalize(request.Slug);
        var index = await _slugIndex.GetBySlugAsync(normalized, cancellationToken);
        if (index is null || !index.IsPublished)
        {
            return Result.Failure<PublicClinicSiteDto>(ClinicSiteError.Site.SlugNotFound);
        }

        var profile = await _profiles.GetAsync(cancellationToken);
        if (profile is null || !profile.IsPublished || !string.Equals(profile.Slug, normalized, StringComparison.Ordinal))
        {
            return Result.Failure<PublicClinicSiteDto>(ClinicSiteError.Site.NotPublished);
        }

        return Result.Success(ClinicSiteMapping.ToPublicDto(profile));
    }
}
