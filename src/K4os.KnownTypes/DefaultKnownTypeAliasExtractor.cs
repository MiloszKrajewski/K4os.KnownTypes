using System.Reflection;
using System.Runtime.Serialization;

namespace K4os.KnownTypes;

/// <summary>
/// Default implementation of <see cref="IKnownTypeAliasExtractor"/> which
/// uses <see cref="KnownTypeAliasAttribute"/> and <see cref="DataContractAttribute"/>
/// as alias sources.
/// </summary>
public class DefaultKnownTypeAliasExtractor: IKnownTypeAliasExtractor
{
    /// <summary>
    /// The default instance of <see cref="DefaultKnownTypeAliasExtractor"/>.
    /// </summary>
    public static readonly DefaultKnownTypeAliasExtractor Instance = new();

    /// <inheritdoc />
    public bool AutoRegister(Type type) =>
        type.GetCustomAttribute(typeof(DataContractAttribute), false) is not null ||
        type.GetCustomAttributes(typeof(KnownTypeAliasAttribute), false).Length > 0;

    /// <inheritdoc />
    public string[] GetAliases(Type type) =>
        ExtractKnownTypeAliases(type).Concat(ExtractDataContractsNames(type))
            .DefaultIfEmpty(FallbackAlias(type))
            .Distinct()
            .ToArray();

    private static IEnumerable<string> ExtractKnownTypeAliases(Type type) =>
        KnownTypeAliasAttribute.EnumerateNames(type);

    private static IEnumerable<string> ExtractDataContractsNames(Type type) =>
        type.GetCustomAttributes(typeof(DataContractAttribute), false)
            .OfType<DataContractAttribute>()
            .Select(a => GetDataContractName(type, a));

    private static string GetDataContractName(Type type, DataContractAttribute attribute)
    {
        var (ns, nm) = (attribute.IsNamespaceSetExplicitly, attribute.IsNameSetExplicitly) switch {
            (false, false) => GetTypeName(type),
            (false, true) => (null, attribute.Name),
            (true, false) => (attribute.Namespace, null),
            (true, true) => (attribute.Namespace, attribute.Name),
        };
        ns = ns is null || string.IsNullOrWhiteSpace(ns) ? null : ns;
        nm = nm is null || string.IsNullOrWhiteSpace(nm) ? type.Name : nm;
        return ns is null ? nm : $"{ns}/{nm}";
    }

    private static (string? Namespace, string Name) GetTypeName(Type type) =>
        (type.Namespace, type.FullName) switch {
            (null, var fn) => (null, fn ?? type.Name),
            (var ns, null) => (ns, type.Name),
            var (ns, fn) when fn.StartsWith($"{ns}.") => (ns, fn.Substring(ns.Length + 1)),
            var (ns, _) => (ns, type.Name)
        };

    private static string FallbackAlias(Type type) =>
        GetTypeName(type) switch { (null, var n) => n, var (ns, n) => $"{ns}/{n}" };
}
