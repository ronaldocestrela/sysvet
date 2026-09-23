using Core.Application.Privacy;
using Core.Application.Privacy.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Privacy;

public class AnonymizeTutorCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAnonymizeTutor_AndInvokeContributors()
    {
        var tutorId = Guid.NewGuid();
        var tutor = Tutor.Create(
            "Maria",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value,
            tutorId).Value;

        var tutorRepository = Substitute.For<ITutorRepository>();
        tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var contributor = Substitute.For<IPersonalDataErasureContributor>();
        contributor.ModuleKey.Returns("Test");
        contributor.EraseForTutorAsync(Arg.Any<PersonalDataErasureContext>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new AnonymizeTutorCommandHandler(tutorRepository, new[] { contributor });

        var result = await handler.Handle(new AnonymizeTutorCommand(tutorId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tutor.IsAnonymized.Should().BeTrue();
        await contributor.Received(1).EraseForTutorAsync(
            Arg.Is<PersonalDataErasureContext>(c => c.OriginalCpf == "12345678909"),
            Arg.Any<CancellationToken>());
        tutorRepository.Received(1).Update(tutor);
    }
}
