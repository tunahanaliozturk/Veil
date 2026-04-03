using FluentAssertions;
using Moongazing.Veil.Extensions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Extensions;

public sealed class StringExtensionTests
{
    [Fact]
    public void Veil_AutoDetectsEmail_MasksCorrectly()
    {
        var result = "john.doe@gmail.com".Veil();

        result.Should().Contain("@");
        result.Should().Contain("*");
        result.Should().NotBe("john.doe@gmail.com");
    }

    [Fact]
    public void Veil_ExplicitPattern_MasksCorrectly()
    {
        var result = "john.doe@gmail.com".Veil(VeilPattern.Email);
        result.Should().Be("j******e@g****.com");
    }

    [Fact]
    public void Veil_FullPattern_MasksEntireString()
    {
        var result = "secret".Veil(VeilPattern.Full);
        result.Should().Be("******");
    }

    [Fact]
    public void Veil_CustomMaskChar_Works()
    {
        var result = "john.doe@gmail.com".Veil(VeilPattern.Email, '#');
        result.Should().Be("j######e@g####.com");
    }

    [Fact]
    public void Veil_NullString_ReturnsNull()
    {
        string? value = null;
        var result = value!.Veil();
        result.Should().BeNull();
    }

    [Fact]
    public void Veil_EmptyString_ReturnsEmpty()
    {
        var result = "".Veil();
        result.Should().BeEmpty();
    }

    [Fact]
    public void RedactAll_TextWithSensitiveData_MasksAll()
    {
        var text = "Email: john@example.com and IP: 192.168.1.100";
        var result = text.RedactAll();

        result.Should().NotContain("john@example.com");
        result.Should().Contain("*");
    }

    [Fact]
    public void RedactAll_PlainText_ReturnsOriginal()
    {
        var result = "no sensitive data here".RedactAll();
        result.Should().Be("no sensitive data here");
    }

    [Fact]
    public void RedactAll_NullString_ReturnsNull()
    {
        string? value = null;
        var result = value!.RedactAll();
        result.Should().BeNull();
    }

    [Fact]
    public void RedactAll_EmptyString_ReturnsEmpty()
    {
        var result = "".RedactAll();
        result.Should().BeEmpty();
    }

    [Fact]
    public void RedactAll_CustomMaskChar_Works()
    {
        var text = "Email: john@example.com";
        var result = text.RedactAll('#');

        result.Should().NotContain("john@example.com");
        result.Should().Contain("#");
    }
}
