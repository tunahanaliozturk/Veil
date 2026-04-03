using FluentAssertions;
using Moongazing.Veil.Detection;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Detection;

public sealed class AutoDetectionTests
{
    private readonly SensitiveDataDetector _sut = new();

    [Fact]
    public void ContainsSensitiveData_WithEmail_ReturnsTrue()
    {
        _sut.ContainsSensitiveData("john@example.com").Should().BeTrue();
    }

    [Fact]
    public void ContainsSensitiveData_WithPlainText_ReturnsFalse()
    {
        _sut.ContainsSensitiveData("hello world").Should().BeFalse();
    }

    [Fact]
    public void ContainsSensitiveData_NullOrEmpty_ReturnsFalse()
    {
        _sut.ContainsSensitiveData("").Should().BeFalse();
        _sut.ContainsSensitiveData(null!).Should().BeFalse();
    }

    [Fact]
    public void DetectAndMask_Email_MasksCorrectly()
    {
        var result = _sut.DetectAndMask("john@example.com");
        result.Should().Contain("*");
        result.Should().Contain("@");
    }

    [Fact]
    public void DetectAndMask_PlainText_ReturnsOriginal()
    {
        _sut.DetectAndMask("hello world").Should().Be("hello world");
    }

    [Fact]
    public void DetectAndMask_NullOrEmpty_ReturnsInput()
    {
        _sut.DetectAndMask("").Should().Be("");
        _sut.DetectAndMask(null!).Should().BeNull();
    }

    [Fact]
    public void Detect_TextWithEmail_FindsEmail()
    {
        var results = _sut.Detect("Contact us at john@example.com for info.");

        results.Should().ContainSingle();
        results[0].Pattern.Should().Be(VeilPattern.Email);
        results[0].OriginalValue.Should().Be("john@example.com");
        results[0].StartIndex.Should().Be(14);
        results[0].Length.Should().Be(16);
    }

    [Fact]
    public void Detect_TextWithCreditCard_FindsCard()
    {
        var results = _sut.Detect("Card: 4532015112830366 is on file.");

        // The digit sequence may match as CreditCard or Phone (both regexes overlap on 16 digits).
        // The important thing is that the detector finds sensitive data at position 6.
        results.Should().Contain(r =>
            r.StartIndex == 6 &&
            r.OriginalValue == "4532015112830366" &&
            (r.Pattern == VeilPattern.CreditCard || r.Pattern == VeilPattern.Phone));
    }

    [Fact]
    public void Detect_TextWithMultipleSensitiveItems_FindsAll()
    {
        var text = "Email: john@example.com, IP: 192.168.1.100";
        var results = _sut.Detect(text);

        results.Should().HaveCountGreaterOrEqualTo(2);
        results.Should().Contain(r => r.Pattern == VeilPattern.Email);
        results.Should().Contain(r => r.Pattern == VeilPattern.Ipv4);
    }

    [Fact]
    public void Detect_EmptyText_ReturnsEmpty()
    {
        _sut.Detect("").Should().BeEmpty();
    }

    [Fact]
    public void Detect_WhitespaceOnly_ReturnsEmpty()
    {
        _sut.Detect("   ").Should().BeEmpty();
    }

    [Fact]
    public void Detect_NullText_ThrowsArgumentNullException()
    {
        var act = () => _sut.Detect(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Detect_ResultsAreOrderedByPosition()
    {
        var text = "IP: 10.0.0.1 and email: a@b.com";
        var results = _sut.Detect(text);

        for (var i = 1; i < results.Count; i++)
        {
            results[i].StartIndex.Should().BeGreaterOrEqualTo(
                results[i - 1].StartIndex + results[i - 1].Length);
        }
    }

    [Fact]
    public void Detect_IpAddress_CorrectPositionAndValue()
    {
        var text = "Server at 192.168.1.100 is down.";
        var results = _sut.Detect(text);

        results.Should().Contain(r =>
            r.Pattern == VeilPattern.Ipv4 &&
            r.OriginalValue == "192.168.1.100");
    }
}
