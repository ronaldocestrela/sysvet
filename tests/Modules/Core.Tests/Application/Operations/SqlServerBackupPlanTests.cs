using Core.Application.Operations;
using Core.Domain;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Application.Operations;

public class SqlServerBackupPlanTests
{
    [Fact]
    public void BuildDrillScript_ShouldIncludeChecksumVerifyOnlyDrillAndDrop()
    {
        var result = SqlServerBackupPlan.BuildDrillScript(new SqlServerBackupPlanRequest(
            "SysVet",
            "/var/backups/sysvet"));

        result.IsSuccess.Should().BeTrue();
        var script = string.Join('\n', result.Value);
        script.Should().Contain("BACKUP DATABASE [SysVet]");
        script.Should().Contain("CHECKSUM");
        script.Should().Contain("RESTORE VERIFYONLY");
        script.Should().Contain("SysVet_drill");
        script.Should().Contain("DROP DATABASE");
        script.Should().Contain("PlatformTenants");
    }

    [Fact]
    public void BuildDrillScript_ShouldFail_WhenDatabaseNameMissing()
    {
        var result = SqlServerBackupPlan.BuildDrillScript(new SqlServerBackupPlanRequest("", "/tmp/backups"));
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Request.InvalidPayload);
    }
}
