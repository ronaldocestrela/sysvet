using Clients.Infrastructure.Persistence;
using FluentAssertions;

namespace Clients.Tests.Persistence;

public class SqliteFilePersistenceTests
{
    [Fact]
    public async Task RestoreIfExistsAsync_ShouldWriteBytesToDatabaseFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sysvet-restore-{Guid.NewGuid()}.db");
        var payload = new byte[] { 1, 2, 3, 4 };
        var persistence = new TestSqliteFilePersistence(
            () => Task.FromResult<byte[]?>(payload)!,
            _ => Task.CompletedTask,
            path);

        await persistence.RestoreIfExistsAsync();

        File.Exists(path).Should().BeTrue();
        var read = await File.ReadAllBytesAsync(path);
        read.Should().Equal(payload);

        File.Delete(path);
    }

    [Fact]
    public void ResolveFilePath_ShouldParseDataSource()
    {
        SqliteFileHelper.ResolveFilePath("Data Source=/tmp/custom.db;Mode=ReadWrite")
            .Should().Be("/tmp/custom.db");
    }

    private sealed class TestSqliteFilePersistence : ISqliteFilePersistence
    {
        private readonly Func<Task<byte[]?>> _load;
        private readonly Func<byte[], Task> _save;
        private readonly string _filePath;

        public TestSqliteFilePersistence(Func<Task<byte[]?>> load, Func<byte[], Task> save, string filePath)
        {
            _load = load;
            _save = save;
            _filePath = filePath;
        }

        public async Task RestoreIfExistsAsync(CancellationToken cancellationToken = default)
        {
            var bytes = await _load();
            if (bytes is { Length: > 0 })
            {
                await File.WriteAllBytesAsync(_filePath, bytes, cancellationToken);
            }
        }

        public Task PersistAsync(byte[] databaseBytes, CancellationToken cancellationToken = default)
            => _save(databaseBytes);
    }
}
