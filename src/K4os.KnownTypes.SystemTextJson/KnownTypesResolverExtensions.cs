using System.Text.Json;

namespace K4os.KnownTypes.SystemTextJson;

/// <summary>
/// Extensions for <see cref="IKnownTypesResolver"/>.
/// </summary>
public static class KnownTypesResolverExtensions
{
    /// <summary>Tries to fix known type.</summary>
    /// <param name="registry"></param>
    /// <param name="options"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static object? TryFixKnownType(
        this IKnownTypesResolver registry, JsonSerializerOptions options, JsonElement element)
    {
        if (!element.TryGetProperty("$type", out var typeAliasProperty))
            return null;

        var typeAlias = typeAliasProperty.GetString();
        if (typeAlias is null)
            return null;

        return TryFixKnownType(registry, options, typeAlias, element);
    }

    private static object? TryFixKnownType(
        IKnownTypesResolver registry, JsonSerializerOptions options,
        string alias, JsonElement element) =>
        registry.TryGetType(alias) switch {
            null => null, var t => element.Deserialize(t, options)
        };
}
