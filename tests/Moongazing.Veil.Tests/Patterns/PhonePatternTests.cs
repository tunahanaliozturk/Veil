using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class PhonePatternTests
{
    private readonly PhonePattern _sut = new();

    [Theory]
    [InlineData("+905551234567")]
    [InlineData("+1 555 123 4567")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("05551234567")]
    [InlineData("+49 30 12345678")]
    [InlineData("(555) 123-4567")]
    public void IsMatch_ValidPhoneNumbers_ReturnsTrue(string phone)
    {
        _sut.IsMatch(phone).Should().BeTrue();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("hello")]
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
    public void Mask_TurkishPhone_KeepsFrontAndBack()
    {
        // "+905551234567" -> digits "905551234567" (12 digits)
        // showFront = min(5, 12-4) = 5, showBack = 4, maskCount = 12-5-4 = 3
        // Result: "+90555***4567"
        var result = _sut.Mask("+905551234567");
        result.Should().Be("+90555***4567");
    }

    [Fact]
    public void Mask_USPhoneWithSpaces_StripsFormattingAndMasks()
    {
        // "+1 555 123 4567" -> digits "15551234567" (11 digits)
        // showFront = min(5, 11-4) = 5, showBack = 4, maskCount = 11-5-4 = 2
        // Result: "+15551**4567"
        var result = _sut.Mask("+1 555 123 4567");
        result.Should().Be("+15551**4567");
    }

    [Fact]
    public void Mask_ShortNumber_FullyMasked()
    {
        // If digits < 7, full mask
        var result = _sut.Mask("+12345");
        result.Should().Be("******");
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
        var result = _sut.Mask("+905551234567", '#');
        result.Should().Be("+90555###4567");
    }

    [Fact]
    public void PatternType_ReturnsPhone()
    {
        _sut.PatternType.Should().Be(VeilPattern.Phone);
    }
}
