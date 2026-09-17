using Clients.Infrastructure;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clients.Tests.Crm;

public class OfflinePetStoreTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private OfflineDbContext _dbContext = null!;
    private OfflineTutorStore _tutorStore = null!;
    private OfflinePetStore _petStore = null!;

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
        _tutorStore = new OfflineTutorStore(tutorRepo, petRepo, _dbContext);
        _petStore = new OfflinePetStore(petRepo, tutorRepo, _dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldRequireActiveTutor()
    {
        var result = await _petStore.CreateAsync(new CreatePetRequest
        {
            Id = Guid.NewGuid(),
            Name = "Lonely",
            Species = PetSpeciesDto.Cat,
            Breed = "SRD",
            Sex = PetSexDto.Female,
            TutorId = Guid.NewGuid()
        });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPet_WhenTutorExists()
    {
        var tutorId = Guid.NewGuid();
        await _tutorStore.CreateAsync(new CreateTutorRequest
        {
            Id = tutorId,
            Name = "Tutor",
            Email = "tutor@test.com",
            Cpf = "12345678909",
            Phone = "11988887777"
        });

        var petId = Guid.NewGuid();
        var create = await _petStore.CreateAsync(new CreatePetRequest
        {
            Id = petId,
            Name = "Mimi",
            Species = PetSpeciesDto.Cat,
            Breed = "Persa",
            Sex = PetSexDto.Female,
            TutorId = tutorId
        });

        create.IsSuccess.Should().BeTrue();
        var list = await _petStore.ListAsync(1, 100);
        list.Value.Items.Should().ContainSingle(p => p.Name == "Mimi");
    }
}
