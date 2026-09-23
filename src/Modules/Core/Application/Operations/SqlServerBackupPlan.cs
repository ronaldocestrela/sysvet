using Core.Domain;

namespace Core.Application.Operations;

/// <summary>Input for generating SQL Server backup and DR drill scripts (10.5).</summary>
public sealed record SqlServerBackupPlanRequest(
    string DatabaseName,
    string BackupDirectory,
    string? LogicalDataName = null,
    string? LogicalLogName = null,
    int RetentionDays = 14);

/// <summary>
/// Pure T-SQL planner for full backup, verify-only restore, scratch restore drill and retention cleanup.
/// Does not open database connections.
/// </summary>
public static class SqlServerBackupPlan
{
    /// <summary>Validates request and builds the ordered script batch.</summary>
    public static Result<IReadOnlyList<string>> BuildDrillScript(SqlServerBackupPlanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DatabaseName))
        {
            return Result.Failure<IReadOnlyList<string>>(ErrorCodes.Request.InvalidPayload);
        }

        if (string.IsNullOrWhiteSpace(request.BackupDirectory))
        {
            return Result.Failure<IReadOnlyList<string>>(ErrorCodes.Request.InvalidPayload);
        }

        var normalizedDirectory = Path.GetFullPath(request.BackupDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (!normalizedDirectory.EndsWith(Path.DirectorySeparatorChar))
        {
            normalizedDirectory += Path.DirectorySeparatorChar;
        }

        var backupFileName = $"{request.DatabaseName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
        var backupPath = Path.GetFullPath(Path.Combine(normalizedDirectory, backupFileName));

        if (!backupPath.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<IReadOnlyList<string>>(ErrorCodes.Request.InvalidPayload);
        }

        var drillDatabase = $"{request.DatabaseName}_drill";
        var dataLogical = request.LogicalDataName ?? request.DatabaseName;
        var logLogical = request.LogicalLogName ?? $"{request.DatabaseName}_log";

        var dataFile = Path.Combine(normalizedDirectory, $"{drillDatabase}.mdf");
        var logFile = Path.Combine(normalizedDirectory, $"{drillDatabase}_log.ldf");

        var escapedBackupPath = EscapeSqlLiteral(backupPath);
        var escapedDataFile = EscapeSqlLiteral(dataFile);
        var escapedLogFile = EscapeSqlLiteral(logFile);

        var scripts = new List<string>
        {
            $"DECLARE @sentinel_before INT = (SELECT COUNT(*) FROM [{request.DatabaseName}].dbo.PlatformTenants);",
            $"BACKUP DATABASE [{request.DatabaseName}] TO DISK = N'{escapedBackupPath}' WITH COMPRESSION, CHECKSUM, STATS = 5;",
            $"RESTORE VERIFYONLY FROM DISK = N'{escapedBackupPath}' WITH CHECKSUM;",
            $"""
             IF DB_ID(N'{drillDatabase}') IS NOT NULL
             BEGIN
                 ALTER DATABASE [{drillDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                 DROP DATABASE [{drillDatabase}];
             END
             """,
            $"""
             RESTORE DATABASE [{drillDatabase}] FROM DISK = N'{escapedBackupPath}'
             WITH MOVE N'{dataLogical}' TO N'{escapedDataFile}',
                  MOVE N'{logLogical}' TO N'{escapedLogFile}',
                  RECOVERY, CHECKSUM;
             """,
            $"DECLARE @sentinel_after INT = (SELECT COUNT(*) FROM [{drillDatabase}].dbo.PlatformTenants);",
            "IF @sentinel_before <> @sentinel_after RAISERROR('DR drill sentinel mismatch.', 16, 1);",
            $"""
             ALTER DATABASE [{drillDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
             DROP DATABASE [{drillDatabase}];
             """
        };

        return Result.Success<IReadOnlyList<string>>(scripts);
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
