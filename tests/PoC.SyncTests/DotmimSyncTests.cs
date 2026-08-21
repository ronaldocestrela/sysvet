using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clients.Infrastructure;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Dotmim.Sync;
using Dotmim.Sync.Sqlite;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PoC.SyncTests;

public class DotmimSyncTests : IDisposable
{
    private readonly string _serverDbPath;
    private readonly string _clientDbPath;
    
    private readonly DbContextOptions<OfflineDbContext> _serverOptions;
    private readonly DbContextOptions<OfflineDbContext> _clientOptions;

    public DotmimSyncTests()
    {
        _serverDbPath = Path.Combine(Path.GetTempPath(), $"server_{Guid.NewGuid()}.db");
        _clientDbPath = Path.Combine(Path.GetTempPath(), $"client_{Guid.NewGuid()}.db");

        _serverOptions = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite($"Data Source={_serverDbPath}")
            .Options;

        _clientOptions = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite($"Data Source={_clientDbPath}")
            .Options;

        using var serverDb = new OfflineDbContext(_serverOptions);
        serverDb.Database.EnsureCreated();

        using var clientDb = new OfflineDbContext(_clientOptions);
        clientDb.Database.EnsureCreated();
    }

    [Fact(Skip = "Requires SQL Server instance to run. Sqlite cannot be used as ServerProvider in Dotmim.Sync.")]
    public async Task Should_Sync_Tutor_From_Client_To_Server()
    {
        // 1. Arrange - Setup Dotmim Sync
        // We will sync the 'Tutor' table.
        var setup = new SyncSetup("Tutors");
        var serverProvider = new SqliteSyncProvider($"Data Source={_serverDbPath}");
        var clientProvider = new SqliteSyncProvider($"Data Source={_clientDbPath}");
        var agent = new SyncAgent(clientProvider, serverProvider);

        // 2. Insert data in Client
        using (var clientDb = new OfflineDbContext(_clientOptions))
        {
            var tutor = Tutor.Create("Client Tutor", Email.Create("client@test.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
            clientDb.Tutors.Add(tutor);
            await clientDb.SaveChangesAsync();
        }

        // 3. Act - Synchronize
        var result = await agent.SynchronizeAsync(setup);

        // 4. Assert
        result.Should().NotBeNull();

        using (var serverDb = new OfflineDbContext(_serverOptions))
        {
            var tutorsInServer = await serverDb.Tutors.ToListAsync();
            tutorsInServer.Should().HaveCount(1);
            tutorsInServer.First().Name.Should().Be("Client Tutor");
        }
    }

    public void Dispose()
    {
        if (File.Exists(_serverDbPath)) File.Delete(_serverDbPath);
        if (File.Exists(_clientDbPath)) File.Delete(_clientDbPath);
    }
}
