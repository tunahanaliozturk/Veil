using Moongazing.Veil.Patterns;

namespace Moongazing.Veil;

/// <summary>
/// Specifies the position of visible characters when masking.
/// </summary>
public enum VeilPosition
{
    /// <summary>
    /// Use the pattern's default positioning strategy.
    /// </summary>
    Default,

    /// <summary>
    /// Keep visible characters at the beginning of the value.
    /// </summary>
    First,

    /// <summary>
    /// Keep visible characters at the end of the value.
    /// </summary>
    Last
}

/// <summary>
/// Marks a property for automatic sensitive data masking when the containing object is veiled.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class VeiledAttribute : Attribute
{
    /// <summary>
    /// Gets the masking pattern to apply to the property value.
    /// </summary>
    public VeilPattern Pattern { get; }

    /// <summary>
    /// Gets or sets the number of characters to leave visible.
    /// A value of <c>-1</c> means the pattern's default behavior is used.
    /// </summary>
    public int Show { get; set; } = -1;

    /// <summary>
    /// Gets or sets the position of visible characters.
    /// </summary>
    public VeilPosition Position { get; set; } = VeilPosition.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="VeiledAttribute"/> class
    /// with automatic pattern detection.
    /// </summary>
    public VeiledAttribute()
    {
        Pattern = VeilPattern.Auto;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VeiledAttribute"/> class
    /// with the specified pattern.
    /// </summary>
    /// <param name="pattern">The masking pattern to apply.</param>
    public VeiledAttribute(VeilPattern pattern)
    {
        Pattern = pattern;
    }
}
