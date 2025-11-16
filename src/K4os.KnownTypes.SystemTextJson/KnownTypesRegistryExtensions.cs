using System.Text.Json;
using System.Text.Json.Serialization;

namespace K4os.KnownTypes.SystemTextJson;

/// <summary><see cref="KnownTypesRegistry"/> extensions for System.Text.Json.</summary>
public static class KnownTypesRegistryExtensions
{
    /// <summary>
    /// Creates minimal <see cref="JsonSerializerOptions"/> with <see cref="KnownTypesJsonTypeInfoResolver"/>.
    /// Most likely you want to configure it yourself, but this is minimal working configuration.
    /// </summary>
    /// <param name="resolver"><see cref="IKnownTypesResolver"/></param>
    /// <returns><see cref="JsonSerializerOptions"/></returns>
    public static JsonSerializerOptions CreateSystemTextJsonOptions(
        this IKnownTypesResolver resolver) =>
        new() { TypeInfoResolver = resolver.CreateJsonTypeInfoResolver() };

    /// <summary>Creates preconfigured <see cref="KnownTypesJsonTypeInfoResolver"/>.</summary>
    /// <param name="resolver"><see cref="IKnownTypesResolver"/></param>
    /// <returns><see cref="KnownTypesJsonTypeInfoResolver"/></returns>
    public static KnownTypesJsonTypeInfoResolver CreateJsonTypeInfoResolver(
        this IKnownTypesResolver resolver) =>
        new(resolver);
    
    /// <summary>Creates <see cref="JsonConverter"/> for <see cref="object"/> fields that supports known types resolution.</summary>
    /// <param name="resolver"><see cref="IKnownTypesResolver"/></param>
    /// <param name="scope"><see cref="ConversionScope"/></param>
    /// <returns><see cref="KnownTypeObjectConverter"/></returns>
    public static JsonConverter CreateKnownObjectConverter(
        this IKnownTypesResolver resolver, ConversionScope scope = ConversionScope.Default) =>
        new KnownTypeObjectConverter(resolver, scope);

}
