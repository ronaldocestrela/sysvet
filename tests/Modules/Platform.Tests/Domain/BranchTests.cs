using FluentAssertions;
using Platform.Domain;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class BranchTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void CreateHeadquarters_WithValidCnpj_Succeeds()
    {
        var result = Branch.CreateHeadquarters(TenantId, "11222333000181", "Matriz LTDA");
        result.IsSuccess.Should().BeTrue();
        result.Value.IsHeadquarters.Should().BeTrue();
        result.Value.Cnpj.Should().Be("11222333000181");
    }

    [Fact]
    public void CreateBranch_WithInvalidCnpj_Fails()
    {
        var result = Branch.CreateBranch(TenantId, "123", "Filial");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Branch.InvalidCnpj.Code);
    }

    [Fact]
    public void CreateBranch_WithoutLegalName_Fails()
    {
        var result = Branch.CreateBranch(TenantId, "11222333000181", " ");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Branch.InvalidLegalName.Code);
    }

    [Fact]
    public void MarkDeleted_SetsDeletedAt()
    {
        var branch = Branch.CreateBranch(TenantId, "11222333000181", "Filial SP").Value;
        branch.MarkDeleted().IsSuccess.Should().BeTrue();
        branch.DeletedAt.Should().NotBeNull();
    }
}
