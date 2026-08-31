using Core.Domain.ValueObjects;
using Clients.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clients.Tests.Maui;

public class MauiIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OfflineDbContext _context;

    public MauiIntegrationTests()
    {
        // Use in-memory SQLite for testing MAUI's local database behavior
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new OfflineDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Should_Save_Offline_Data_Successfully()
    {
        // Arrange
        var emailResult = Email.Create("joao@test.com");
        var cpfResult = Cpf.Create("12345678909");
        var phoneResult = Phone.Create("11999998888");
        
        var tutorResult = Core.Domain.Entities.Tutor.Create(
            "João Silva",
            emailResult.Value,
            cpfResult.Value,
            phoneResult.Value
        );
        var tutor = tutorResult.Value;

        // Act
        _context.Tutors.Add(tutor);
        await _context.SaveChangesAsync();
        
        var savedData = await _context.Tutors.FirstOrDefaultAsync(x => x.Id == tutor.Id);
        var outboxMessage = await _context.OutboxMessages.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(savedData);
        Assert.Equal("João Silva", savedData.Name);
        Assert.NotNull(outboxMessage);
        Assert.Equal("RegisterTutorCommand", outboxMessage.Type);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
