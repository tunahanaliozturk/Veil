using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.ObjectMasking;

public sealed class AttributeMaskingTests
{
    [Fact]
    public void MaskObject_VeiledEmailProperty_IsMasked()
    {
        var dto = new UserDto
        {
            Name = "John Doe",
            Email = "john.doe@gmail.com",
            Phone = "+905551234567"
        };

        var masked = Veil.MaskObject(dto);

        masked.Email.Should().NotBe("john.doe@gmail.com");
        masked.Email.Should().Contain("@");
        masked.Email.Should().Contain("*");
    }

    [Fact]
    public void MaskObject_VeiledPhoneProperty_IsMasked()
    {
        var dto = new UserDto
        {
            Name = "John Doe",
            Email = "john.doe@gmail.com",
            Phone = "+905551234567"
        };

        var masked = Veil.MaskObject(dto);

        masked.Phone.Should().NotBe("+905551234567");
        masked.Phone.Should().Contain("*");
    }

    [Fact]
    public void MaskObject_NonVeiledProperty_IsUnchanged()
    {
        var dto = new UserDto
        {
            Name = "John Doe",
            Email = "john.doe@gmail.com",
            Phone = "+905551234567"
        };

        var masked = Veil.MaskObject(dto);

        masked.Name.Should().Be("John Doe");
    }

    [Fact]
    public void MaskObject_OriginalObject_IsNotModified()
    {
        var dto = new UserDto
        {
            Name = "John Doe",
            Email = "john.doe@gmail.com",
            Phone = "+905551234567"
        };

        _ = Veil.MaskObject(dto);

        dto.Email.Should().Be("john.doe@gmail.com");
        dto.Phone.Should().Be("+905551234567");
        dto.Name.Should().Be("John Doe");
    }

    [Fact]
    public void MaskObject_NullProperty_IsHandledGracefully()
    {
        var dto = new UserDto
        {
            Name = "John Doe",
            Email = null!,
            Phone = null!
        };

        var act = () => Veil.MaskObject(dto);
        act.Should().NotThrow();

        var masked = Veil.MaskObject(dto);
        masked.Email.Should().BeNull();
        masked.Phone.Should().BeNull();
    }

    [Fact]
    public void MaskObject_NullInput_ThrowsArgumentNullException()
    {
        var act = () => Veil.MaskObject<UserDto>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MaskObject_FullMaskPattern_MasksEntireValue()
    {
        var dto = new SecretDto { Password = "my-super-secret-password" };

        var masked = Veil.MaskObject(dto);

        masked.Password.Should().Be("************************");
    }

    [Fact]
    public void MaskObject_AutoPattern_DetectsAndMasks()
    {
        var dto = new AutoDetectDto { SomeValue = "john@example.com" };

        var masked = Veil.MaskObject(dto);

        masked.SomeValue.Should().Contain("@");
        masked.SomeValue.Should().Contain("*");
    }

    private sealed class UserDto
    {
        public string Name { get; set; } = string.Empty;

        [Veiled(VeilPattern.Email)]
        public string Email { get; set; } = string.Empty;

        [Veiled(VeilPattern.Phone)]
        public string Phone { get; set; } = string.Empty;
    }

    private sealed class SecretDto
    {
        [Veiled(VeilPattern.Full)]
        public string Password { get; set; } = string.Empty;
    }

    private sealed class AutoDetectDto
    {
        [Veiled]
        public string SomeValue { get; set; } = string.Empty;
    }
}
