using System.Runtime.CompilerServices;
using System.Text.Json;

namespace K4os.KnownTypes.SystemTextJson.Internal;

internal struct JsonElementParser
{
    private readonly JsonSerializerOptions _options;
    private readonly IKnownTypesResolver _resolver;
    private readonly ConversionScope _mode;

    public JsonElementParser(
        JsonSerializerOptions options,
        IKnownTypesResolver resolver,
        ConversionScope mode = ConversionScope.Default)
    {
        _options = options;
        _resolver = resolver;
        _mode = mode;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEnabled(ConversionScope mode) =>
        (_mode & mode) == mode;

    public object? ToObject(JsonElement element) =>
        element.ValueKind switch {
            JsonValueKind.Object => ToKnownObject(element) ?? ToDictionary(element) ?? element,
            JsonValueKind.Array => ToArray(element) ?? element,
            JsonValueKind.String => ToString(element) ?? element,
            JsonValueKind.Number => ToNumber(element) ?? element,
            JsonValueKind.True => ToBoolean(true) ?? element,
            JsonValueKind.False => ToBoolean(false) ?? element,
            JsonValueKind.Null or JsonValueKind.Undefined => ToNull(element)/* NO! ?? element */,
            _ => element
        };

    private object? ToObject(JsonProperty property) =>
        ToObject(property.Value);

    private object? ToKnownObject(JsonElement element) =>
        IsEnabled(ConversionScope.KnownObjects)
            ? TryGetKnownType(element) switch { { } tt => element.Deserialize(tt, _options), _ => null }
            : null;

    private Type? TryGetKnownType(JsonElement element) =>
        element.TryGetProperty(JsonConfiguration.TypePropertyName, out var property) &&
        property.ValueKind == JsonValueKind.String
            ? property.GetString() switch { { } tn => _resolver.TryGetType(tn), _ => null }
            : null;

    private object? ToDictionary(JsonElement element) =>
        IsEnabled(ConversionScope.Dictionaries)
            ? element.EnumerateObject().ToDictionary(p => p.Name, ToObject)
            : null;

    private object? ToArray(JsonElement element) =>
        IsEnabled(ConversionScope.Arrays)
            ? element.EnumerateArray().Select(ToObject).ToArray()
            : null;

    private object? ToNumber(JsonElement element) =>
        IsEnabled(ConversionScope.Integers) && element.TryGetInt64(out var l) ? l :
        IsEnabled(ConversionScope.Floats) && element.TryGetDouble(out var d) ? d :
        null;
    
    private object? ToBoolean(bool value) =>
        IsEnabled(ConversionScope.Booleans) ? value : null;

    private object? ToString(JsonElement element) =>
        ToString(element.GetString());

    private object? ToString(string? text) =>
        IsEnabled(ConversionScope.Guids) && Guid.TryParse(text, out var guid) ? guid :
        IsEnabled(ConversionScope.Timestamps) && DateTime.TryParse(text, out var dt) ? dt :
        text;
    
    private object? ToNull(JsonElement element) =>
        IsEnabled(ConversionScope.Nulls) ? null : element;
}
