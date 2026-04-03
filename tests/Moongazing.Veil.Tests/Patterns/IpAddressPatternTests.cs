using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.Patterns;

public sealed class IpAddressPatternTests
{
    private readonly IpAddressPattern _sut = new();

    [Theory]
    [InlineData("192.168.1.100")]
    [InlineData("10.0.0.1")]
    [InlineData("255.255.255.255")]
    [InlineData("0.0.0.0")]
    [InlineData("172.16.254.1")]
    public void IsMatch_ValidIpv4Addresses_ReturnsTrue(string ip)
    {
        _sut.IsMatch(ip).Should().BeTrue();
    }

    [Theory]
    [InlineData("256.1.1.1")]
    [InlineData("1.2.3")]
    [InlineData("not.an.ip.address")]
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
    public void Mask_ValidIp_KeepsFirstTwoOctets()
    {
        var result = _sut.Mask("192.168.1.100");
        result.Should().Be("192.168.*.*");
    }

    [Fact]
    public void Mask_AnotherIp_KeepsFirstTwoOctets()
    {
        var result = _sut.Mask("10.0.0.1");
        result.Should().Be("10.0.*.*");
    }

    [Fact]
    public void Mask_CustomMaskChar()
    {
        var result = _sut.Mask("192.168.1.100", '#');
        result.Should().Be("192.168.#.#");
    }

    [Fact]
    public void Mask_NotFourParts_FullMask()
    {
        var result = _sut.Mask("192.168.1");
        result.Should().Be("*********");
    }

    [Fact]
    public void Mask_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _sut.Mask(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PatternType_ReturnsIpv4()
    {
        _sut.PatternType.Should().Be(VeilPattern.Ipv4);
    }
}
