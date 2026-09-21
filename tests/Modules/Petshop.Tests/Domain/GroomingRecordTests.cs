using FluentAssertions;
using Petshop.Domain.Entities;
using Xunit;

namespace Petshop.Tests.Domain;

public class GroomingRecordTests
{
    [Fact]
    public void SeedSuppliesFromService_CopiesDefaultLines()
    {
        var serviceId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var service = GroomingService.Create(serviceId, "Banho", Petshop.Domain.Enums.GroomingServiceType.Banho, 30, "Banho").Value;
        service.SetDefaultSupplies([(productId, 0.25m)]);

        var record = GroomingRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        var seed = record.SeedSuppliesFromService(service);

        seed.IsSuccess.Should().BeTrue();
        record.SupplyLines.Should().ContainSingle(l => l.ProductId == productId && l.Quantity == 0.25m);
    }

    [Fact]
    public void FinalizeRecord_WhenDraft_ShouldLockRecord()
    {
        var record = GroomingRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = record.FinalizeRecord();

        result.IsSuccess.Should().BeTrue();
        record.Status.Should().Be(GroomingRecordStatus.Finalized);
        record.UpdateCoatNotes("x").IsFailure.Should().BeTrue();
    }
}
