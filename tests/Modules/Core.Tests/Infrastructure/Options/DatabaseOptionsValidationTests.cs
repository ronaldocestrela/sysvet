using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Core.Tests.Infrastructure.Options;

public class DatabaseOptionsValidationTests
{
    [Fact]
    public void DatabaseOptions_ShouldBeValid_WithSupportedProvider()
    {
        var options = new DatabaseOptions
        {
            Provider = "Sqlite",
            ConnectionStringName = "DefaultConnection"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void DatabaseOptions_ShouldBeInvalid_WhenProviderMissing()
    {
        var options = new DatabaseOptions
        {
            Provider = "",
            ConnectionStringName = "DefaultConnection"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        isValid.Should().BeFalse();
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void TenancySettings_ShouldBeInvalid_WhenDefaultSchemaMissing()
    {
        var options = new TenancySettings { DefaultSchema = "" };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, new ValidationContext(options), results, true);

        isValid.Should().BeFalse();
    }
}

public class ModuleConnectionStringResolverTests
{
    [Fact]
    public void Resolve_ShouldUseDefaultConnection_WhenModuleOverrideIsEmpty()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=shared.db"
            })
            .Build();

        var databaseOptions = new DatabaseOptions();

        var result = ModuleConnectionStringResolver.Resolve(configuration, databaseOptions, moduleConnectionStringOverride: null);

        result.Should().Be("Data Source=shared.db");
    }

    [Fact]
    public void Resolve_ShouldPreferModuleOverride_WhenSet()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=shared.db"
            })
            .Build();

        var databaseOptions = new DatabaseOptions();

        var result = ModuleConnectionStringResolver.Resolve(
            configuration,
            databaseOptions,
            moduleConnectionStringOverride: "Data Source=veterinary-only.db");

        result.Should().Be("Data Source=veterinary-only.db");
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenConnectionStringMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var databaseOptions = new DatabaseOptions();

        var act = () => ModuleConnectionStringResolver.Resolve(configuration, databaseOptions, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultConnection*");
    }
}

public class EntityFrameworkConfigurationExtensionsTests
{
    [Fact]
    public void ConfigureModuleDatabase_ShouldThrow_WhenProviderIsUnknown()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Core.Infrastructure.Persistence.CoreDbContext>();
        var databaseOptions = new DatabaseOptions { Provider = "PostgreSql" };

        var act = () => optionsBuilder.ConfigureModuleDatabase(configuration, databaseOptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PostgreSql*");
    }
}
