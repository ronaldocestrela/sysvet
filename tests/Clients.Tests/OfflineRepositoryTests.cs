using System;
using System.Linq;
using System.Threading.Tasks;
using Clients.Infrastructure;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clients.Tests;

public class OfflineRepositoryTests : IDisposable
{
    private readonly OfflineDbContext _dbContext;
    private readonly OfflineRepository<Tutor> _repository;

    public OfflineRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _dbContext = new OfflineDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        _repository = new OfflineRepository<Tutor>(_dbContext);
    }

    [Fact]
    public async Task AddAsync_ShouldSaveTutorToDatabase()
    {
        // Arrange
        var cpf = Cpf.Create("12345678909").Value;
        var email = Email.Create("test@tutor.com").Value;
        var phone = Phone.Create("11999999999").Value;
        var tutor = Tutor.Create("John Doe", email, cpf, phone).Value;

        // Act
        await _repository.AddAsync(tutor);
        await _repository.SaveChangesAsync();

        // Assert
        var dbTutor = await _dbContext.Tutors.FirstOrDefaultAsync(t => t.Id == tutor.Id);
        dbTutor.Should().NotBeNull();
        dbTutor!.Name.Should().Be("John Doe");
        dbTutor.Cpf.Number.Should().Be("12345678909");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTutor_WhenExists()
    {
        // Arrange
        var cpf = Cpf.Create("12345678909").Value;
        var email = Email.Create("test@tutor.com").Value;
        var phone = Phone.Create("11999999999").Value;
        var tutor = Tutor.Create("Jane Doe", email, cpf, phone).Value;
        _dbContext.Tutors.Add(tutor);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tutor.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane Doe");
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }
}
