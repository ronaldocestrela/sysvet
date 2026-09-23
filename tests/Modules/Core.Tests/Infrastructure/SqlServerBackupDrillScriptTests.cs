using FluentAssertions;
using Xunit;

namespace Core.Tests.Infrastructure;

public class SqlServerBackupDrillScriptTests
{
    [Fact]
    public void DrillScript_Should_ContainRequiredMarkers()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "scripts",
            "sqlserver-backup-restore-drill.sh"));

        File.Exists(path).Should().BeTrue("drill script must exist at repo scripts/");
        var content = File.ReadAllText(path);
        content.Should().Contain("CHECKSUM");
        content.Should().Contain("RESTORE VERIFYONLY");
        content.Should().Contain("_drill");
        content.Should().Contain("DROP DATABASE");
        content.Should().Contain("PlatformTenants");
    }
}
