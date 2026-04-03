namespace Moongazing.Veil.Extensions;

/// <summary>
/// Extension methods for masking sensitive properties on objects.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Creates a masked copy of this object. Properties marked with <see cref="VeiledAttribute"/>
    /// or matching convention rules will be masked. The original object is not modified.
    /// </summary>
    /// <typeparam name="T">The type of the object to mask.</typeparam>
    /// <param name="obj">The object whose properties should be masked.</param>
    /// <returns>A new instance of <typeparamref name="T"/> with sensitive properties masked.</returns>
    public static T VeilProperties<T>(this T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        return Veil.MaskObject(obj);
    }
}
