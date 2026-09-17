using Clients.Infrastructure;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Repositories;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clients.Tests.Crm;

public class OfflineTutorStoreTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private OfflineDbContext _dbContext = null!;
    private OfflineTutorStore _store = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new OfflineDbContext(options, new NoOpSqliteFilePersistence());
        await _dbContext.Database.MigrateAsync();

        var tutorRepo = new OfflineTutorRepository(_dbContext);
        var petRepo = new OfflinePetRepository(_dbContext);
        _store = new OfflineTutorStore(tutorRepo, petRepo, _dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistLocally_WithoutHttp()
    {
        var id = Guid.NewGuid();
        var result = await _store.CreateAsync(new CreateTutorRequest
        {
            Id = id,
            Name = "Maria Silva",
            Email = "maria@test.com",
            Cpf = "12345678909",
            Phone = "11988887777"
        });

        result.IsSuccess.Should().BeTrue();
        var list = await _store.ListAsync(1, 100);
        list.Value.Items.Should().ContainSingle(t => t.Name == "Maria Silva");
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeStoredTutor()
    {
        var id = Guid.NewGuid();
        await _store.CreateAsync(new CreateTutorRequest
        {
            Id = id,
            Name = "Before",
            Email = "before@test.com",
            Cpf = "12345678909",
            Phone = "11988887777"
        });

        var update = await _store.UpdateAsync(new UpdateTutorRequest
        {
            Id = id,
            Name = "After",
            Email = "after@test.com",
            Phone = "11977776666"
        });

        update.IsSuccess.Should().BeTrue();
        var get = await _store.GetByIdAsync(id);
        get.Value.Name.Should().Be("After");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTutorAndPets()
    {
        var tutorId = Guid.NewGuid();
        await _store.CreateAsync(new CreateTutorRequest
        {
            Id = tutorId,
            Name = "Tutor",
            Email = "tutor@test.com",
            Cpf = "12345678909",
            Phone = "11988887777"
        });

        var petStore = new OfflinePetStore(
            new OfflinePetRepository(_dbContext),
            new OfflineTutorRepository(_dbContext),
            _dbContext);

        await petStore.CreateAsync(new CreatePetRequest
        {
            Id = Guid.NewGuid(),
            Name = "Rex",
            Species = PetSpeciesDto.Dog,
            Breed = "SRD",
            Sex = PetSexDto.Male,
            TutorId = tutorId
        });

        var delete = await _store.DeleteAsync(tutorId);
        delete.IsSuccess.Should().BeTrue();

        var list = await _store.ListAsync(1, 100);
        list.Value.Items.Should().BeEmpty();

        var rawTutor = await _dbContext.Tutors.IgnoreQueryFilters().FirstAsync(t => t.Id == tutorId);
        rawTutor.IsDeleted.Should().BeTrue();
    }
}
