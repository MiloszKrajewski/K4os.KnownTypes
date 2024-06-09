using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace K4os.KnownTypes.NewtonsoftJson;

/// <summary><see cref="KnownTypesRegistry"/> extensions for Newtonsoft.Json.</summary>
public static class KnownTypesRegistryExtensions
{
    /// <summary>Creates minimal <see cref="JsonSerializerSettings"/> with
    /// <see cref="KnownTypesSerializationBinder"/>. Most likely you want to
    /// configure it yourself, but this is minimal working configuration.</summary>
    /// <param name="registry"><see cref="KnownTypesRegistry"/></param>
    /// <returns><see cref="JsonSerializerSettings"/></returns>
    public static JsonSerializerSettings CreateNewtonsoftJsonSettings(
        this KnownTypesRegistry registry) =>
        new() {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = registry.CreateJsonSerializationBinder()
        };

    /// <summary>Creates preconfigured <see cref="KnownTypesSerializationBinder"/>.</summary>
    /// <param name="registry"><see cref="KnownTypesRegistry"/></param>
    /// <returns><see cref="KnownTypesSerializationBinder"/></returns>
    public static ISerializationBinder CreateJsonSerializationBinder(
        this KnownTypesRegistry registry) =>
        new KnownTypesSerializationBinder(registry);
}
