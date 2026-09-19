using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.MedicalRecords.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class MedicalRecordUpdateCommandHandlerTests
{
    private static MedicalRecord CreateRecord() =>
        MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

    private static (IMedicalRecordRepository Repo, IAuditLogger Audit, ITenantContext Tenant) Deps(MedicalRecord? record)
    {
        var repo = Substitute.For<IMedicalRecordRepository>();
        var audit = Substitute.For<IAuditLogger>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(record);
        return (repo, audit, tenant);
    }

    [Fact]
    public async Task RecordVitalSigns_WhenDraft_Succeeds()
    {
        var record = CreateRecord();
        var (repo, audit, tenant) = Deps(record);
        var handler = new RecordVitalSignsCommandHandler(repo, audit, tenant);

        var result = await handler.Handle(new RecordVitalSignsCommand(
            record.Id, 12.5m, 38.5m, 90, 20, "pink", "<2s", DateTimeOffset.UtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Update(record);
    }

    [Fact]
    public async Task RecordVitalSigns_WhenMissing_ReturnsNotFound()
    {
        var (repo, audit, tenant) = Deps(null);
        var handler = new RecordVitalSignsCommandHandler(repo, audit, tenant);

        var result = await handler.Handle(new RecordVitalSignsCommand(
            Guid.NewGuid(), 12.5m, 38.5m, 90, 20, "pink", "<2s", DateTimeOffset.UtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.NotFound");
    }

    [Fact]
    public async Task AddEvolutionNote_AppendsNote()
    {
        var record = CreateRecord();
        var (repo, audit, tenant) = Deps(record);
        var handler = new AddEvolutionNoteCommandHandler(repo, audit, tenant);

        var result = await handler.Handle(new AddEvolutionNoteCommand(record.Id, "Improved"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        record.EvolutionNotes.Should().HaveCount(1);
    }

    [Fact]
    public async Task SetDiagnosis_AndSetConduct_UpdateFields()
    {
        var record = CreateRecord();
        var (repo, audit, tenant) = Deps(record);

        (await new SetDiagnosisCommandHandler(repo, audit, tenant)
            .Handle(new SetDiagnosisCommand(record.Id, "Gastritis"), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await new SetConductCommandHandler(repo, audit, tenant)
            .Handle(new SetConductCommand(record.Id, "Diet"), CancellationToken.None)).IsSuccess.Should().BeTrue();

        record.Diagnosis.Should().Be("Gastritis");
        record.Prescription.Should().Be("Diet");
    }
}
