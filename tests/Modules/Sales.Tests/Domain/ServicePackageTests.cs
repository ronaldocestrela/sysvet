using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Tests.Domain;

public class ServicePackageTests
{
    [Fact]
    public void Create_WithZeroUses_ReturnsFailure()
    {
        var result = ServicePackage.Create("Pacote", ServiceCode.Banho, 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Package.InvalidUsesPerUnit");
    }
}
