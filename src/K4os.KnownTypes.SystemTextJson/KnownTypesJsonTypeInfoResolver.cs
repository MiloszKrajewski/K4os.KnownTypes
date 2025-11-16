using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using K4os.KnownTypes.SystemTextJson.Internal;

namespace K4os.KnownTypes.SystemTextJson;

/// <summary>TypeInfo resolver supporting <see cref="KnownTypesRegistry"/>.</summary>
public class KnownTypesJsonTypeInfoResolver: DefaultJsonTypeInfoResolver
{
    private readonly IKnownTypesResolver _registry;
    
    /// <summary>Types resolver.</summary>
    public IKnownTypesResolver KnownTypesResolver => _registry;

    /// <summary>Creates new instance of <see cref="KnownTypesJsonTypeInfoResolver"/>.
    /// If no registry is provided uses shared (default) one.</summary>
    /// <param name="registry"></param>
    public KnownTypesJsonTypeInfoResolver(IKnownTypesResolver? registry = null) => 
        _registry = registry ?? KnownTypesRegistry.Default;

    /// <inheritdoc />
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object) 
            return jsonTypeInfo;

        // Clone cached polymorphism options to avoid shared mutations across different JsonTypeInfo instances.
        var polymorphismOptions = Clone(GetOrCreatePolymorphismOptions(jsonTypeInfo.Type));
        if (polymorphismOptions is not null)
        {
            jsonTypeInfo.PolymorphismOptions = polymorphismOptions;
        }

        return jsonTypeInfo;
    }

    private readonly ConcurrentDictionary<Type, JsonPolymorphismOptions?>
        _polymorphismOptionsCache = new();

    private JsonPolymorphismOptions? GetOrCreatePolymorphismOptions(Type type) =>
        _polymorphismOptionsCache.GetOrAdd(type, CreatePolymorphismOptions);

    private JsonPolymorphismOptions? CreatePolymorphismOptions(Type type)
    {
        var knownDerivedTypes = FindKnownDerivedTypes(type).ToList();
        if (knownDerivedTypes.Count <= 0)
            return null;

        var options = new JsonPolymorphismOptions {
            TypeDiscriminatorPropertyName = JsonConfiguration.TypePropertyName,
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };
        knownDerivedTypes.ForEach(options.DerivedTypes.Add);
        return options;
    }

    private static JsonPolymorphismOptions? Clone(JsonPolymorphismOptions? other)
    {
        if (other is null)
            return null;

        var clone = new JsonPolymorphismOptions {
            TypeDiscriminatorPropertyName = other.TypeDiscriminatorPropertyName,
            IgnoreUnrecognizedTypeDiscriminators = other.IgnoreUnrecognizedTypeDiscriminators,
            UnknownDerivedTypeHandling = other.UnknownDerivedTypeHandling,
        };

        foreach (var derivedType in other.DerivedTypes)
            clone.DerivedTypes.Add(derivedType);

        return clone;
    }

    private IEnumerable<JsonDerivedType> FindKnownDerivedTypes(Type rootType) =>
        from type in _registry.KnownTypes
        where rootType.IsAssignableFrom(type)
        let alias = _registry.TryGetAlias(type)
        where alias is not null
        select new JsonDerivedType(type, alias);
}
