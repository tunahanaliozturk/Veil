using FluentAssertions;
using Moongazing.Veil.ObjectMasking;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.ObjectMasking;

public sealed class ConventionMaskingTests
{
    [Fact]
    public void ConventionBuilder_WhenPropertyNameMatches_AppliesPattern()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(name => name.Contains("Password", StringComparison.OrdinalIgnoreCase))
            .UsePattern(VeilPattern.Full);

        var rules = conventions.GetRules();
        rules.Should().HaveCount(1);
        rules[0].Predicate("Password").Should().BeTrue();
        rules[0].Predicate("Name").Should().BeFalse();
        rules[0].Pattern.Should().Be(VeilPattern.Full);
    }

    [Fact]
    public void ConventionBuilder_MultipleRules_AllRegistered()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(n => n.Contains("Email", StringComparison.OrdinalIgnoreCase))
            .UsePattern(VeilPattern.Email);
        conventions.WhenPropertyName(n => n.Contains("Phone", StringComparison.OrdinalIgnoreCase))
            .UsePattern(VeilPattern.Phone);

        conventions.GetRules().Should().HaveCount(2);
    }

    [Fact]
    public void ConventionBuilder_NullPredicate_ThrowsArgumentNullException()
    {
        var conventions = new ConventionBuilder();
        var act = () => conventions.WhenPropertyName(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PropertyMaskResolver_ConventionRule_AppliesToMatchingProperty()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(n => n == "Secret")
            .UsePattern(VeilPattern.Full);

        var resolver = new PropertyMaskResolver(conventions, enableAutoDetection: false);

        var property = typeof(TestConventionDto).GetProperty(nameof(TestConventionDto.Secret))!;
        var result = resolver.Resolve(property, "somevalue");

        result.Should().Be(VeilPattern.Full);
    }

    [Fact]
    public void PropertyMaskResolver_NoMatchingConvention_ReturnsNull()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(n => n == "Password")
            .UsePattern(VeilPattern.Full);

        var resolver = new PropertyMaskResolver(conventions, enableAutoDetection: false);

        var property = typeof(TestConventionDto).GetProperty(nameof(TestConventionDto.Name))!;
        var result = resolver.Resolve(property, "somevalue");

        result.Should().BeNull();
    }

    [Fact]
    public void PropertyMaskResolver_AttributeTakesPriorityOverConvention()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(n => n == "Email")
            .UsePattern(VeilPattern.Full);

        var resolver = new PropertyMaskResolver(conventions, enableAutoDetection: false);

        var property = typeof(TestConventionDto).GetProperty(nameof(TestConventionDto.Email))!;
        var result = resolver.Resolve(property, "john@example.com");

        // [Veiled(VeilPattern.Email)] takes priority over convention Full
        result.Should().Be(VeilPattern.Email);
    }

    [Fact]
    public void ObjectMasker_WithConventions_MasksByPropertyName()
    {
        var conventions = new ConventionBuilder();
        conventions.WhenPropertyName(n => n == "Secret")
            .UsePattern(VeilPattern.Full);

        var resolver = new PropertyMaskResolver(conventions, enableAutoDetection: false);
        var masker = new ObjectMasker(VeilPatternRegistry.Default, resolver);

        var dto = new TestConventionDto
        {
            Name = "John",
            Secret = "my-api-secret-value",
            Email = "john@example.com"
        };

        var masked = masker.MaskObject(dto);

        masked.Secret.Should().Be("*******************");
        masked.Name.Should().Be("John");
    }

    private sealed class TestConventionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;

        [Veiled(VeilPattern.Email)]
        public string Email { get; set; } = string.Empty;
    }
}
