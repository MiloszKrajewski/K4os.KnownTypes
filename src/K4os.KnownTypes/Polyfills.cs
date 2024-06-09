#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#else
using System.Collections.Immutable;
#endif

using System.Runtime.CompilerServices;

namespace K4os.KnownTypes;

internal static class Polyfills
{
#if !NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V? GetValueOrDefault<K, V>(this IReadOnlyDictionary<K, V> dictionary, K key) =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    public static bool TryAdd<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
            return false;

        dictionary.Add(key, value);
        return true;
    }
#endif

#if NET8_0_OR_GREATER
    public static FrozenDictionary<K, V> Freeze<K, V>(
        this Dictionary<K, V> source) where K: notnull =>
        source.ToFrozenDictionary();
#else
    public static ImmutableDictionary<K, V> Freeze<K, V>(
        this Dictionary<K, V> source) where K: notnull =>
        source.ToImmutableDictionary();
#endif
    
#if NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNull<T>(
        this T? value, [CallerArgumentExpression(nameof(value))] string? expression = null) 
        where T: class =>
        value ?? ThrowArgumentNullExpression<T>(expression);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNull<T>(this T? value, string? expression = null) 
        where T: class =>
        value ?? ThrowArgumentNullExpression<T>(null);
#endif

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowArgumentNullExpression<T>(string? expression) where T: class => 
        throw new ArgumentNullException($"'{expression ?? "<expression>"}' cannot be null");
}
