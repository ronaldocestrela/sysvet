using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Core.Tests.Infrastructure.Persistence;

public class CoreDbContextTests
{
    private DbContextOptions<CoreDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldIsolateDataByTenant()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        var tenantContext1 = Substitute.For<ITenantContext>();
        tenantContext1.TenantId.Returns(tenant1Id);
        tenantContext1.SchemaName.Returns("tenant1");

        var tenantContext2 = Substitute.For<ITenantContext>();
        tenantContext2.TenantId.Returns(tenant2Id);
        tenantContext2.SchemaName.Returns("tenant2");

        var options = CreateOptions("TestDb_Isolation");

        var email = Email.Create("test@test.com").Value;
        var cpf = Cpf.Create("12345678909").Value;
        var phone = Phone.Create("11999999999").Value;

        // Act - Save data in tenant 1
        using (var context1 = new CoreDbContext(options, tenantContext1))
        {
            var tutor = Tutor.Create("Tutor Tenant 1", email, cpf, phone).Value;
            context1.Tutors.Add(tutor);
            await context1.SaveChangesAsync();
        }

        // Assert - Tenant 2 cannot see Tenant 1 data
        using (var context2 = new CoreDbContext(options, tenantContext2))
        {
            var tutors = await context2.Tutors.ToListAsync();
            tutors.Should().BeEmpty();
        }
    }
}
