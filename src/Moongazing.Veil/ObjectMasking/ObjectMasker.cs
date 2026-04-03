using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.ObjectMasking;

/// <summary>
/// Masks sensitive properties on objects using reflection with aggressive type metadata caching.
/// Creates shallow copies of objects so originals are never modified.
/// </summary>
public sealed class ObjectMasker
{
    private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> TypeCache = new();
    private static readonly MethodInfo MemberwiseCloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private readonly VeilPatternRegistry _registry;
    private readonly PropertyMaskResolver _resolver;
    private readonly char _maskChar;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectMasker"/> class.
    /// </summary>
    /// <param name="registry">The pattern registry.</param>
    /// <param name="resolver">The property mask resolver.</param>
    /// <param name="maskChar">The character used for masking.</param>
    public ObjectMasker(VeilPatternRegistry registry, PropertyMaskResolver resolver, char maskChar = '*')
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _maskChar = maskChar;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectMasker"/> class with default settings.
    /// </summary>
    public ObjectMasker()
        : this(VeilPatternRegistry.Default, new PropertyMaskResolver(), '*')
    {
    }

    /// <summary>
    /// Creates a masked copy of the specified object. The original object is not modified.
    /// </summary>
    /// <typeparam name="T">The type of the object to mask.</typeparam>
    /// <param name="obj">The object to mask.</param>
    /// <returns>A shallow copy of the object with marked properties masked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <see langword="null"/>.</exception>
    public T MaskObject<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        return (T)MaskObjectInternal(obj, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    /// <summary>
    /// Gets all properties decorated with <see cref="VeiledAttribute"/> for the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>An array of property information for veiled properties.</returns>
    public static PropertyInfo[] GetVeiledProperties(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var metadata = GetCachedProperties(type);
        return metadata
            .Where(m => m.Attribute is not null)
            .Select(m => m.Property)
            .ToArray();
    }

    private object MaskObjectInternal(object obj, HashSet<object> visited)
    {
        var type = obj.GetType();

        // Prevent infinite recursion on circular references
        if (!visited.Add(obj))
        {
            return obj;
        }

        // Handle collections
        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            return MaskCollection(enumerable, type, visited);
        }

        // Create a shallow copy
        var clone = MemberwiseCloneMethod.Invoke(obj, null)!;

        var properties = GetCachedProperties(type);

        foreach (var meta in properties)
        {
            if (!meta.Property.CanRead)
            {
                continue;
            }

            var value = meta.Property.GetValue(clone);

            if (value is null)
            {
                continue;
            }

            if (meta.Property.PropertyType == typeof(string))
            {
                var stringValue = (string)value;
                var pattern = _resolver.Resolve(meta.Property, stringValue);

                if (pattern.HasValue && meta.Property.CanWrite)
                {
                    var masked = MaskValue(stringValue, pattern.Value);
                    meta.Property.SetValue(clone, masked);
                }
            }
            else if (!meta.Property.PropertyType.IsValueType)
            {
                if (!meta.Property.CanWrite)
                {
                    continue;
                }

                if (value is IEnumerable childEnumerable)
                {
                    var maskedCollection = MaskCollection(childEnumerable, meta.Property.PropertyType, visited);
                    meta.Property.SetValue(clone, maskedCollection);
                }
                else
                {
                    var maskedChild = MaskObjectInternal(value, visited);
                    meta.Property.SetValue(clone, maskedChild);
                }
            }
        }

        return clone;
    }

    private object MaskCollection(IEnumerable enumerable, Type collectionType, HashSet<object> visited)
    {
        var elementType = GetElementType(collectionType);
        if (elementType is null)
        {
            return enumerable;
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var item in enumerable)
        {
            if (item is null)
            {
                list.Add(null);
            }
            else if (item is string)
            {
                list.Add(item); // strings in raw collections are not auto-masked
            }
            else if (!elementType.IsValueType)
            {
                list.Add(MaskObjectInternal(item, visited));
            }
            else
            {
                list.Add(item);
            }
        }

        return list;
    }

    private string MaskValue(string value, VeilPattern patternType)
    {
        if (patternType == VeilPattern.Auto)
        {
            var detected = _registry.TryDetect(value);
            if (detected is null)
            {
                return value;
            }

            return detected.Mask(value, _maskChar);
        }

        var pattern = _registry.GetPattern(patternType);
        return pattern is not null ? pattern.Mask(value, _maskChar) : value;
    }

    private static PropertyMetadata[] GetCachedProperties(Type type)
    {
        return TypeCache.GetOrAdd(type, static t =>
        {
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var metadata = new PropertyMetadata[props.Length];

            for (var i = 0; i < props.Length; i++)
            {
                var attr = props[i].GetCustomAttribute<VeiledAttribute>();
                metadata[i] = new PropertyMetadata(props[i], attr);
            }

            return metadata;
        });
    }

    private static Type? GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        foreach (var iface in collectionType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private sealed record PropertyMetadata(PropertyInfo Property, VeiledAttribute? Attribute);
}
