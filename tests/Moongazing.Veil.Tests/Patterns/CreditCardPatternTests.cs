using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class CreditCardPatternTests
{
    private readonly CreditCardPattern _sut = new();

    [Theory]
    [InlineData("4532015112830366")]
    [InlineData("5425 1234 5678 9012")]
    [InlineData("4532-0151-1283-0366")]
    public void IsMatch_ValidCardNumbers_ReturnsTrue(string card)
    {
        _sut.IsMatch(card).Should().BeTrue();
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("abcdefghijklmnop")]
    [InlineData("hello world")]
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
    public void Mask_SixteenDigitNoSpaces_KeepsFirstFourLastFour()
    {
        var result = _sut.Mask("4532015112830366");
        result.Should().Be("4532********0366");
    }

    [Fact]
    public void Mask_WithSpaces_PreservesSpacesAndMasksMiddle()
    {
        var result = _sut.Mask("5425 1234 5678 9012");
        result.Should().Be("5425 **** **** 9012");
    }

    [Fact]
    public void Mask_WithDashes_PreservesDashesAndMasksMiddle()
    {
        var result = _sut.Mask("4532-0151-1283-0366");
        result.Should().Be("4532-****-****-0366");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Mask_CustomMaskChar_UsesProvidedChar()
    {
        var result = _sut.Mask("4532015112830366", '#');
        result.Should().Be("4532########0366");
    }

    [Fact]
    public void Mask_TooFewDigits_FullMask()
    {
        // Less than 8 digits -> full mask
        var result = _sut.Mask("1234567");
        result.Should().Be("*******");
    }

    [Fact]
    public void PatternType_ReturnsCreditCard()
    {
        _sut.PatternType.Should().Be(VeilPattern.CreditCard);
    }
}
