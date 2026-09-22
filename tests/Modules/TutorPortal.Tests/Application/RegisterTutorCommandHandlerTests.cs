using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.Auth.Commands;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Tests.Application;

public class RegisterTutorCommandHandlerTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly ICrmTutorLookup _crmTutorLookup = Substitute.For<ICrmTutorLookup>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITutorPortalAccountRepository _accountRepository = Substitute.For<ITutorPortalAccountRepository>();
    private readonly ITutorPortalUnitOfWork _unitOfWork = Substitute.For<ITutorPortalUnitOfWork>();
    private readonly TutorPortalAuthService _authService;

    public RegisterTutorCommandHandlerTests()
    {
        _tenantContext.TenantId.Returns(Guid.NewGuid());
        _authService = new TutorPortalAuthService(
            Substitute.For<IAccessTokenIssuer>(),
            Substitute.For<IRefreshTokenStore>(),
            _accountRepository);
    }

    [Fact]
    public async Task Handle_WhenEmailAndCpfMismatch_ShouldReturnRegistrationDenied()
    {
        _crmTutorLookup.FindActiveTutorByEmailAndCpfAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CrmTutorMatch?)null);

        var handler = new RegisterTutorCommandHandler(
            _tenantContext,
            _crmTutorLookup,
            _identityService,
            _accountRepository,
            _unitOfWork,
            _authService);

        var result = await handler.Handle(
            new RegisterTutorCommand("a@test.com", "52998224725", "Password123!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.RegistrationDenied");
    }
}
