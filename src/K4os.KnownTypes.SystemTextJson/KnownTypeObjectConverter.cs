using System.Text.Json;
using System.Text.Json.Serialization;
using K4os.KnownTypes.SystemTextJson.Internal;

namespace K4os.KnownTypes.SystemTextJson;

/// <summary>
/// Dedicated converter for <see cref="object"/> fields that supports known types resolution.
/// By default System.Text.Json does not support polymorphic deserialization for <see cref="object"/> fields
/// and populates them with <see cref="JsonElement"/>. This converter uses <see cref="IKnownTypesResolver"/>
/// to resolve actual type and deserialize accordingly.
/// </summary>
public class KnownTypeObjectConverter: JsonConverter<object>
{
    private readonly IKnownTypesResolver _resolver;
    private readonly ConversionScope _scope;

    /// <summary>Creates new instance of <see cref="KnownTypeObjectConverter"/>.</summary>
    /// <param name="resolver">Known types resolver.</param>
    /// <param name="scope">Conversion scope.</param>
    public KnownTypeObjectConverter(IKnownTypesResolver resolver, ConversionScope scope = ConversionScope.Default)
    {
        _resolver = resolver;
        _scope = scope;
    }

    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var parser = new JsonElementParser(options, _resolver, _scope);
        return parser.ToObject(document.RootElement);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var actualType = value.GetType();
        if (actualType == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        JsonSerializer.Serialize(writer, value, actualType, options);
    }
}
