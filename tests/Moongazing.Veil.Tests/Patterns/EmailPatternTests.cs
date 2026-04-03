using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class EmailPatternTests
{
    private readonly EmailPattern _sut = new();

    [Theory]
    [InlineData("john.doe@gmail.com")]
    [InlineData("user@example.org")]
    [InlineData("first.last@sub.domain.co.uk")]
    [InlineData("test+tag@company.com")]
    [InlineData("a@b.io")]
    [InlineData("user123@test-domain.com")]
    [InlineData("name%special@domain.net")]
    public void IsMatch_ValidEmails_ReturnsTrue(string email)
    {
        _sut.IsMatch(email).Should().BeTrue();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@domain.com")]
    [InlineData("missing-at-sign.com")]
    [InlineData("hello world")]
    [InlineData("12345")]
    [InlineData("")]
    public void IsMatch_InvalidStrings_ReturnsFalse(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            // Empty string will throw ArgumentNullException for empty check, but IsMatch expects non-null
            // Actually the regex won't match empty string
        }

        _sut.IsMatch(input).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.IsMatch(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Mask_StandardEmail_MasksLocalPartAndDomain()
    {
        // "john.doe@gmail.com" -> local "john.doe" (8 chars): j + 6 mask + e, domain "gmail" -> g + 4 mask, tld ".com"
        var result = _sut.Mask("john.doe@gmail.com");
        result.Should().Be("j******e@g****.com");
    }

    [Fact]
    public void Mask_ShortLocalPart_SingleChar_ReturnsCorrectly()
    {
        // Local part "a" (1 char) -> just "a", domain "b" (1 char) -> just "b"
        var result = _sut.Mask("a@b.io");
        result.Should().Be("a@b.io");
    }

    [Fact]
    public void Mask_TwoCharLocalPart_KeepsFirstAndLast()
    {
        // "ab" -> a + (0 mask chars) + b = "ab"
        var result = _sut.Mask("ab@gmail.com");
        result.Should().Be("ab@g****.com");
    }

    [Fact]
    public void Mask_MultipleDomainDots_MasksOnlyDomainName()
    {
        // "user@sub.domain.co.uk" -> domain part is "sub.domain.co.uk"
        // last dot index finds ".uk", so domain name is "sub.domain.co", tld is ".uk"
        var result = _sut.Mask("user@sub.domain.co.uk");
        result.Should().Be("u**r@s************.uk");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Mask_InputWithoutAtSign_MasksEntireString()
    {
        var result = _sut.Mask("noemail");
        result.Should().Be("*******");
    }

    [Fact]
    public void Mask_CustomMaskChar_UsesProvidedChar()
    {
        var result = _sut.Mask("john.doe@gmail.com", '#');
        result.Should().Be("j######e@g####.com");
    }

    [Fact]
    public void PatternType_ReturnsEmail()
    {
        _sut.PatternType.Should().Be(VeilPattern.Email);
    }

    [Fact]
    public void GetRegex_ReturnsNonNullRegex()
    {
        EmailPattern.GetRegex().Should().NotBeNull();
    }
}
