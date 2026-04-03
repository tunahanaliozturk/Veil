using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class TurkishIdPatternTests
{
    private readonly TurkishIdPattern _sut = new();

    [Theory]
    [InlineData("12345678901")]
    [InlineData("98765432109")]
    [InlineData("55512345678")]
    public void IsMatch_ValidElevenDigitStartingNonZero_ReturnsTrue(string tcNo)
    {
        _sut.IsMatch(tcNo).Should().BeTrue();
    }

    [Theory]
    [InlineData("01234567890")]   // starts with 0
    [InlineData("1234567890")]    // 10 digits
    [InlineData("123456789012")]  // 12 digits
    [InlineData("abcdefghijk")]   // non-digits
    [InlineData("12345")]         // too short
    public void IsMatch_InvalidInputs_ReturnsFalse(string input)
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
    public void Mask_ValidTcNo_KeepsFirst3AndLast3()
    {
        // "12345678901" (11 chars) -> "123" + 5 mask + "901"
        var result = _sut.Mask("12345678901");
        result.Should().Be("123*****901");
    }

    [Fact]
    public void Mask_ShortInput_FullMask()
    {
        var result = _sut.Mask("12345");
        result.Should().Be("*****");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Mask_CustomMaskChar()
    {
        var result = _sut.Mask("12345678901", '#');
        result.Should().Be("123#####901");
    }

    [Fact]
    public void PatternType_ReturnsTurkishId()
    {
        _sut.PatternType.Should().Be(VeilPattern.TurkishId);
    }
}
