using System.Text.Json;
using Core.Application.Pets.Commands;
using Core.Application.Sync;
using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Sync;

public class PushSyncBatchCommandHandlerTests
{
    private readonly IMediator _mediator;
    private readonly PushSyncBatchCommandHandler _handler;

    public PushSyncBatchCommandHandlerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _handler = new PushSyncBatchCommandHandler(_mediator);
    }

    [Fact]
    public async Task Handle_ShouldStopOnFirstError_AndReturnProcessedPrefix()
    {
        var tutorId = Guid.NewGuid();
        var okMessage = new SyncOutboxMessageDto
        {
            Id = Guid.NewGuid(),
            Type = nameof(CreateTutorCommand),
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Payload = JsonSerializer.Serialize(new CreateTutorCommand(tutorId, "A", "a@test.com", "12345678909", "11999999999"))
        };
        var failMessage = new SyncOutboxMessageDto
        {
            Id = Guid.NewGuid(),
            Type = nameof(UpdateTutorCommand),
            CreatedAt = DateTimeOffset.UtcNow,
            Payload = JsonSerializer.Serialize(new UpdateTutorCommand(Guid.NewGuid(), "B", "b@test.com", "11888887777"))
        };

        _mediator.Send(Arg.Any<CreateTutorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(tutorId));
        _mediator.Send(Arg.Any<UpdateTutorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCodes.Tutor.NotFound));

        var result = await _handler.Handle(new PushSyncBatchCommand([okMessage, failMessage]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProcessedIds.Should().ContainSingle().Which.Should().Be(okMessage.Id);
        result.Value.FailedMessageId.Should().Be(failMessage.Id);
        result.Value.IsPermanentFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldInjectIdempotencyKey_FromOutboxId()
    {
        var messageId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var message = new SyncOutboxMessageDto
        {
            Id = messageId,
            Type = nameof(CreateTutorCommand),
            CreatedAt = DateTimeOffset.UtcNow,
            Payload = JsonSerializer.Serialize(new CreateTutorCommand(tutorId, "A", "a@test.com", "12345678909", "11999999999"))
        };

        CreateTutorCommand? captured = null;
        _mediator.Send(Arg.Do<CreateTutorCommand>(c => captured = c), Arg.Any<CancellationToken>())
            .Returns(Result.Success(tutorId));

        await _handler.Handle(new PushSyncBatchCommand([message]), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.IdempotencyKey.Should().Be(messageId);
    }
}
