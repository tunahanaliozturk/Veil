using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class IbanPatternTests
{
    private readonly IbanPattern _sut = new();

    [Theory]
    [InlineData("TR330006100519786457841326")]
    [InlineData("DE89370400440532013000")]
    [InlineData("GB29NWBK60161331926819")]
    [InlineData("FR7630006000011234567890189")]
    public void IsMatch_ValidIbans_ReturnsTrue(string iban)
    {
        _sut.IsMatch(iban).Should().BeTrue();
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("hello")]
    [InlineData("AB12")]
    public void IsMatch_InvalidStrings_ReturnsFalse(string input)
    {
        _sut.IsMatch(input).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.IsMatch(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Mask_TurkishIban_KeepsFirst4AndLast2()
    {
        // "TR330006100519786457841326" (26 chars) -> "TR33" + 20 mask + "26"
        var result = _sut.Mask("TR330006100519786457841326");
        result.Should().Be("TR33********************26");
    }

    [Fact]
    public void Mask_GermanIban_KeepsFirst4AndLast2()
    {
        // "DE89370400440532013000" (22 chars) -> "DE89" + 16 mask + "00"
        var result = _sut.Mask("DE89370400440532013000");
        result.Should().Be("DE89****************00");
    }

    [Fact]
    public void Mask_IbanWithSpaces_StripsSpaces()
    {
        // Spaces are removed, then masked
        var result = _sut.Mask("TR33 0006 1005 1978 6457 8413 26");
        result.Should().Be("TR33********************26");
    }

    [Fact]
    public void Mask_ShortInput_FullMask()
    {
        var result = _sut.Mask("AB12C");
        result.Should().Be("*****");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PatternType_ReturnsIban()
    {
        _sut.PatternType.Should().Be(VeilPattern.Iban);
    }
}
