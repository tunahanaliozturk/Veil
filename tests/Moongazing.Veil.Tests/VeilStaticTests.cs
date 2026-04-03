using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests;

public sealed class VeilStaticTests
{
    [Fact]
    public void Mask_AutoDetectsEmail_MasksCorrectly()
    {
        var result = Veil.Mask("john.doe@gmail.com");

        result.Should().Contain("@");
        result.Should().Contain("*");
        result.Should().NotBe("john.doe@gmail.com");
    }

    [Fact]
    public void Mask_ExplicitEmailPattern_MasksCorrectly()
    {
        var result = Veil.Mask("john.doe@gmail.com", VeilPattern.Email);
        result.Should().Be("j******e@g****.com");
    }

    [Fact]
    public void Mask_ExplicitCreditCardPattern_MasksCorrectly()
    {
        var result = Veil.Mask("4532015112830366", VeilPattern.CreditCard);
        result.Should().Be("4532********0366");
    }

    [Fact]
    public void Mask_FullPattern_MasksEntireString()
    {
        var result = Veil.Mask("sensitive-data", VeilPattern.Full);
        result.Should().Be("**************");
    }

    [Fact]
    public void Mask_NullInput_ReturnsNull()
    {
        var result = Veil.Mask(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Mask_EmptyInput_ReturnsEmpty()
    {
        var result = Veil.Mask("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Mask_CustomMaskChar_UsesProvidedChar()
    {
        var result = Veil.Mask("john.doe@gmail.com", VeilPattern.Email, '#');
        result.Should().Be("j######e@g####.com");
    }

    [Fact]
    public void Mask_AutoWithNoMatch_ReturnsOriginal()
    {
        var result = Veil.Mask("hello");
        result.Should().Be("hello");
    }

    [Fact]
    public void Redact_TextWithSensitiveData_MasksAllOccurrences()
    {
        var text = "Contact john@example.com at 192.168.1.100";
        var result = Veil.Redact(text);

        result.Should().NotContain("john@example.com");
        result.Should().Contain("@");
        result.Should().Contain("*");
    }

    [Fact]
    public void Redact_PlainText_ReturnsOriginal()
    {
        var result = Veil.Redact("just plain text here");
        result.Should().Be("just plain text here");
    }

    [Fact]
    public void Redact_NullInput_ReturnsNull()
    {
        var result = Veil.Redact(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Redact_EmptyInput_ReturnsEmpty()
    {
        var result = Veil.Redact("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Redact_MultiLineText_MasksSensitiveDataAcrossLines()
    {
        var text = "Line1: john@example.com\nLine2: 192.168.1.100\nLine3: clean";
        var result = Veil.Redact(text);

        result.Should().NotContain("john@example.com");
        result.Should().Contain("Line3: clean");
    }

    [Fact]
    public void MaskObject_ReturnsNewMaskedInstance()
    {
        var dto = new TestDto { Email = "test@example.com", Name = "Test" };

        var masked = Veil.MaskObject(dto);

        masked.Should().NotBeSameAs(dto);
        masked.Name.Should().Be("Test");
        masked.Email.Should().Contain("*");
    }

    [Fact]
    public void MaskObject_NullInput_ThrowsArgumentNullException()
    {
        var act = () => Veil.MaskObject<TestDto>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class TestDto
    {
        public string Name { get; set; } = string.Empty;

        [Veiled(VeilPattern.Email)]
        public string Email { get; set; } = string.Empty;
    }
}
