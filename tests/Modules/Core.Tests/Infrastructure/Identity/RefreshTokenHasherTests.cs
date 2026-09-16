using Core.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Infrastructure.Identity;

public class RefreshTokenHasherTests
{
    [Fact]
    public void Hash_ShouldBeDeterministic()
    {
        var a = RefreshTokenHasher.Hash("same-token");
        var b = RefreshTokenHasher.Hash("same-token");
        a.Should().Be(b);
        a.Should().NotBe(RefreshTokenHasher.Hash("other-token"));
    }
}
