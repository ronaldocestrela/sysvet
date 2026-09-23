using Core.Application.Common;
using Core.Domain;
using Xunit;

namespace Core.Tests.Application.Common;

public sealed class PageRequestTests
{
    [Fact]
    public void TryCreate_RejectsPageBelowOne()
    {
        var result = PageRequest.TryCreate(0, 20);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Pagination.InvalidPage.Code, result.Error.Code);
    }

    [Fact]
    public void TryCreate_RejectsPageSizeAboveMax()
    {
        var result = PageRequest.TryCreate(1, 101);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Pagination.PageSizeTooLarge.Code, result.Error.Code);
    }

    [Fact]
    public void TryCreate_DefaultsInvalidPageSize()
    {
        var result = PageRequest.TryCreate(2, 0);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(PageRequest.DefaultPageSize, result.Value.PageSize);
    }
}
