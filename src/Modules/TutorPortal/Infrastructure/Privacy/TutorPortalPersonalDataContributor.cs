using Core.Application.Privacy;
using Core.Domain;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using TutorPortal.Domain.Repositories;
namespace TutorPortal.Infrastructure.Privacy;

/// <summary>Tutor portal linkage and identity erasure for anonymized tutors.</summary>
public sealed class TutorPortalPersonalDataContributor : IPersonalDataExportContributor, IPersonalDataErasureContributor
{
    private readonly ITutorPortalAccountRepository _accountRepository;
    private readonly UserManager<AppUser> _userManager;
    /// <summary>Initializes dependencies.</summary>
    public TutorPortalPersonalDataContributor(
        ITutorPortalAccountRepository accountRepository,
        UserManager<AppUser> userManager)
    {
        _accountRepository = accountRepository;
        _userManager = userManager;
    }

    /// <inheritdoc />
    public string ModuleKey => "TutorPortal";

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByTutorIdAsync(tutorId, cancellationToken);
        return new Dictionary<string, object?>
        {
            ["portalAccount"] = account is null
                ? null
                : new Dictionary<string, object?>
                {
                    ["id"] = account.Id,
                    ["userId"] = account.UserId,
                    ["tutorId"] = account.TutorId
                }
        };
    }

    /// <inheritdoc />
    public async Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByTutorIdAsync(context.TutorId, cancellationToken);
        if (account is null)
        {
            return Result.Success();
        }

        var user = await _userManager.FindByIdAsync(account.UserId);
        if (user is not null)
        {
            await _userManager.SetEmailAsync(user, context.TombstoneEmail);
            await _userManager.SetUserNameAsync(user, context.TombstoneEmail);
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        _accountRepository.Remove(account);
        return Result.Success();
    }
}
