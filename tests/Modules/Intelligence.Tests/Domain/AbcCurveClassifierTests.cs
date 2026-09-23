using FluentAssertions;
using Intelligence.Domain.Reports;

namespace Intelligence.Tests.Domain;

public class AbcCurveClassifierTests
{
    [Fact]
    public void Classify_SingleItem_IsClassA()
    {
        var id = Guid.NewGuid();
        var result = AbcCurveClassifier.Classify([new AbcCurveInputRow(id, 100m, id.ToString("D"))]);

        result.Should().ContainSingle();
        result[0].Class.Should().Be(AbcClass.A);
        result[0].SharePercent.Should().Be(100m);
    }

    [Fact]
    public void Classify_ZeroTotal_ReturnsEmpty()
    {
        var id = Guid.NewGuid();
        var result = AbcCurveClassifier.Classify([new AbcCurveInputRow(id, 0m, id.ToString("D"))]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Classify_StandardSplit_AssignsABC()
    {
        var a = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var b = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var c = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var result = AbcCurveClassifier.Classify(
        [
            new AbcCurveInputRow(a, 50m, a.ToString("D")),
            new AbcCurveInputRow(b, 30m, b.ToString("D")),
            new AbcCurveInputRow(c, 20m, c.ToString("D"))
        ]);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(a);
        result[0].Class.Should().Be(AbcClass.A);
        result[1].Class.Should().Be(AbcClass.A);
        result[2].Class.Should().Be(AbcClass.B);
    }

    [Fact]
    public void Classify_EqualValues_TieBreaksByOrderKey()
    {
        var first = Guid.Parse("00000000-0000-0000-0000-00000000000a");
        var second = Guid.Parse("00000000-0000-0000-0000-00000000000b");

        var result = AbcCurveClassifier.Classify(
        [
            new AbcCurveInputRow(second, 50m, second.ToString("D")),
            new AbcCurveInputRow(first, 50m, first.ToString("D"))
        ]);

        result[0].Id.Should().Be(first);
        result[1].Id.Should().Be(second);
    }
}
