using System.Text.Json;

namespace K4os.KnownTypes.SystemTextJson;

/// <summary><see cref="KnownTypesRegistry"/> extensions for System.Text.Json.</summary>
public static class KnownTypesRegistryExtensions
{
    /// <summary>
    /// Creates minimal <see cref="JsonSerializerOptions"/> with <see cref="KnownTypesJsonTypeInfoResolver"/>.
    /// Most likely you want to configure it yourself, but this is minimal working configuration.
    /// </summary>
    /// <param name="registry"><see cref="KnownTypesRegistry"/></param>
    /// <returns><see cref="JsonSerializerOptions"/></returns>
    public static JsonSerializerOptions CreateSystemTextJsonOptions(
        this KnownTypesRegistry registry) =>
        new() { TypeInfoResolver = registry.CreateJsonTypeInfoResolver() };

    /// <summary>Creates preconfigured <see cref="KnownTypesJsonTypeInfoResolver"/>.</summary>
    /// <param name="registry"><see cref="KnownTypesRegistry"/></param>
    /// <returns><see cref="KnownTypesJsonTypeInfoResolver"/></returns>
    public static KnownTypesJsonTypeInfoResolver CreateJsonTypeInfoResolver(
        this KnownTypesRegistry registry) =>
        new(registry);
}
