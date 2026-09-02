using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Core.Tests.Infrastructure.Persistence;

public class TestTenantContext : ITenantContext
{
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string SchemaName { get; set; } = "tenant_1";
    public string ConnectionString { get; set; } = string.Empty;
}

public class RepositoryTests
{
    private (DbContextOptions<CoreDbContext> Options, CoreDbContext Context) CreateNewContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new CoreDbContext(options, new TestTenantContext());
        
        context.Database.EnsureCreated();

        return (options, context);
    }

    [Fact]
    public async Task AddAsync_Should_AddEntityToDatabase()
    {
        // Arrange
        var setup = CreateNewContext();
        await using var context = setup.Context;
        var repository = new TutorRepository(context);
        
        var tutor = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;

        // Act
        repository.Add(tutor);
        await context.SaveChangesAsync();

        // Assert
        var savedTutor = await context.Tutors.FirstOrDefaultAsync(t => t.Id == tutor.Id);
        savedTutor.Should().NotBeNull();
        savedTutor!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnEntity_When_ItExists()
    {
        // Arrange
        var setup = CreateNewContext();
        await using var context = setup.Context;
        
        var tutor = Tutor.Create("Jane Doe", Email.Create("jane@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
        context.Tutors.Add(tutor);
        await context.SaveChangesAsync();

        var repository = new TutorRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tutor.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_When_NotExists()
    {
        // Arrange
        var setup = CreateNewContext();
        await using var context = setup.Context;
        var repository = new TutorRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}
