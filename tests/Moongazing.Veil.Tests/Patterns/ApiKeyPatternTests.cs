using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class ApiKeyPatternTests
{
    private readonly ApiKeyPattern _sut = new();

    [Theory]
    [InlineData("sk-abc123def456ghi789")]
    [InlineData("pk-1234567890abcdef")]
    [InlineData("key-abcdefghij1234")]
    [InlineData("api-ABCDEFGHIJ1234")]
    [InlineData("token-abcdef1234gh")]
    [InlineData("secret_abcdefghij12")]
    public void IsMatch_PrefixedApiKeys_ReturnsTrue(string key)
    {
        _sut.IsMatch(key).Should().BeTrue();
    }

    [Theory]
    [InlineData("randomstring")]
    [InlineData("sk-short")]
    [InlineData("hello world")]
    [InlineData("12345")]
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
    public void Mask_StandardKey_KeepsPrefixAndFirst3Last3()
    {
        // "sk-abc123def456ghi789" -> prefix "sk-", body "abc123def456ghi789" (18 chars)
        // first 3 "abc" + 12 mask + last 3 "789"
        var result = _sut.Mask("sk-abc123def456ghi789");
        result.Should().Be("sk-abc************789");
    }

    [Fact]
    public void Mask_ShortBody_FullBodyMask()
    {
        // body <= 6 chars -> fully masked
        var result = _sut.Mask("sk-abcdef");
        result.Should().Be("sk-******");
    }

    [Fact]
    public void Mask_UnderscoreSeparator_WorksCorrectly()
    {
        // "secret_abcdefghij12" -> prefix "secret_", body "abcdefghij12" (12 chars)
        // first 3 "abc" + 6 mask + last 3 "j12"
        var result = _sut.Mask("secret_abcdefghij12");
        result.Should().Be("secret_abc******j12");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PatternType_ReturnsApiKey()
    {
        _sut.PatternType.Should().Be(VeilPattern.ApiKey);
    }
}
