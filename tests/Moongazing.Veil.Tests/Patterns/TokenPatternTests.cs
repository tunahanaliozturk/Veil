using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class TokenPatternTests
{
    private readonly TokenPattern _sut = new();

    [Theory]
    [InlineData("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U")]
    public void IsMatch_JwtTokens_ReturnsTrue(string token)
    {
        _sut.IsMatch(token).Should().BeTrue();
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456789")]  // 36 chars base64-like
    public void IsMatch_LongBase64String_ReturnsTrue(string token)
    {
        _sut.IsMatch(token).Should().BeTrue();
    }

    [Theory]
    [InlineData("short")]
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
    public void Mask_BearerToken_PreservesBearerPrefix()
    {
        var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var result = _sut.Mask(token);

        result.Should().StartWith("Bearer eyJh");
        result.Should().Contain("*");
    }

    [Fact]
    public void Mask_StandaloneJwt_ShowsFirst4Chars()
    {
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var result = _sut.Mask(jwt);

        result.Should().StartWith("eyJh");
        result.Length.Should().Be(jwt.Length);
        result[4..].Should().MatchRegex(@"^\*+$");
    }

    [Fact]
    public void Mask_ShortToken_FullyMasked()
    {
        // Token body <= 8 chars (after stripping Bearer) -> fully masked
        var result = _sut.Mask("Bearer 12345678");
        result.Should().Be("Bearer ********");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PatternType_ReturnsToken()
    {
        _sut.PatternType.Should().Be(VeilPattern.Token);
    }
}
