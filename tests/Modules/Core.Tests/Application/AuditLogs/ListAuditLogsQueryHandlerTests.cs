using Core.Application.AuditLogs.Queries;
using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.AuditLogs;

public class ListAuditLogsQueryHandlerTests
{
    private readonly IAuditLogRepository _repository;
    private readonly ListAuditLogsQueryHandler _handler;

    public ListAuditLogsQueryHandlerTests()
    {
        _repository = Substitute.For<IAuditLogRepository>();
        _handler = new ListAuditLogsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedAuditLogs()
    {
        var tenantUserId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var log = AuditLog.Create(Guid.NewGuid(), tenantUserId, entityId, "Tutor", "Modified", "{}").Value;

        _repository.SearchAsync(1, 20, "Tutor", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new PagedList<AuditLog>(new[] { log }, 1));

        var result = await _handler.Handle(new ListAuditLogsQuery(EntityName: "Tutor"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(i => i.EntityName == "Tutor" && i.EntityId == entityId);
    }
}
