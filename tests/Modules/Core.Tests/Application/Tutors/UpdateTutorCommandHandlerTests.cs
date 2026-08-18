using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class UpdateTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Core.Domain.Auditing.IAuditLogger _auditLogger;
    private readonly Core.Domain.ITenantContext _tenantContext;
    private readonly UpdateTutorCommandHandler _handler;

    public UpdateTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _auditLogger = Substitute.For<Core.Domain.Auditing.IAuditLogger>();
        _tenantContext = Substitute.For<Core.Domain.ITenantContext>();
        _handler = new UpdateTutorCommandHandler(_tutorRepository, _unitOfWork, _auditLogger, _tenantContext);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tutorResult = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, id);
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tutorResult.Value);

        var command = new UpdateTutorCommand(id, "John Smith", "smith@example.com", "12345678909", "11888888888");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tutorRepository.Received(1).Update(Arg.Any<Tutor>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var command = new UpdateTutorCommand(id, "John Smith", "smith@example.com", "12345678909", "11888888888");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tutor.NotFound");
        _tutorRepository.DidNotReceive().Update(Arg.Any<Tutor>());
    }
}
