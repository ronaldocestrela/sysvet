using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Core.Tests.Infrastructure.Persistence;

public class CoreDbContextAuditCaptureTests
{
    private static CoreDbContext CreateContext(string dbName, Guid tenantId, Guid userId)
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(tenantId);
        tenantContext.UserId.Returns(userId);
        tenantContext.SchemaName.Returns("tenant_test");

        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new CoreDbContext(options, tenantContext);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTutorAdded_ShouldCreateAuditLogWithUserId()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = CreateContext(nameof(SaveChangesAsync_WhenTutorAdded_ShouldCreateAuditLogWithUserId), tenantId, userId);

        var tutor = Tutor.Create(
            "John Doe",
            Email.Create("john@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999999999").Value).Value;

        context.Tutors.Add(tutor);
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        logs.Should().ContainSingle(l =>
            l.EntityName == "Tutor"
            && l.Action == "Added"
            && l.UserId == userId
            && l.EntityId == tutor.Id
            && l.TenantId == tenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTutorSoftDeleted_ShouldRecordDeletedAction()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = CreateContext(nameof(SaveChangesAsync_WhenTutorSoftDeleted_ShouldRecordDeletedAction), tenantId, userId);

        var tutor = Tutor.Create(
            "John Doe",
            Email.Create("john@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999999999").Value).Value;

        context.Tutors.Add(tutor);
        await context.SaveChangesAsync();

        tutor.SoftDelete();
        context.Tutors.Update(tutor);
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.IgnoreQueryFilters()
            .Where(l => l.EntityId == tutor.Id)
            .OrderBy(l => l.OccurredAt)
            .ToListAsync();

        logs.Should().HaveCount(2);
        logs.Last().Action.Should().Be("Deleted");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAppUserChanges_ShouldNotCreateAuditLog()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = CreateContext(nameof(SaveChangesAsync_WhenAppUserChanges_ShouldNotCreateAuditLog), tenantId, userId);

        context.Users.Add(new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            TenantId = tenantId
        });

        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task AuditLogRepository_SearchAsync_ShouldReturnCapturedEntries()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = CreateContext(nameof(AuditLogRepository_SearchAsync_ShouldReturnCapturedEntries), tenantId, userId);

        var tutor = Tutor.Create(
            "John Doe",
            Email.Create("john@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999999999").Value).Value;

        context.Tutors.Add(tutor);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);
        var page = await repository.SearchAsync(1, 20, "Tutor", null, null, null, null);

        page.TotalCount.Should().Be(1);
        page.Items[0].EntityId.Should().Be(tutor.Id);
    }
}
