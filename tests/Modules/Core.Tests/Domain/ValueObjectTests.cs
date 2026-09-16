using Core.Domain;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Domain;

public class ValueObjectTests
{
    private sealed record TestValue(string Value) : ValueObject;

    [Fact]
    public void TwoValueObjects_WithSameComponents_ShouldBeEqual()
    {
        var a = new TestValue("same");
        var b = new TestValue("same");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TwoValueObjects_WithDifferentComponents_ShouldNotBeEqual()
    {
        var a = new TestValue("a");
        var b = new TestValue("b");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Cpf_ShouldBeValueObject()
    {
        typeof(Cpf).Should().BeAssignableTo<ValueObject>();
    }
}
