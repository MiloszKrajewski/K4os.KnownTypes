using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace K4os.KnownTypes;

/// <summary>
/// Know types registry storing list of well-known type aliases to, for example,
/// avoid usage of fully qualified type names.
/// </summary>
public class KnownTypesRegistry: IKnownTypesRegistry, IKnownTypesResolver
{
    /// <summary>The default <see cref="KnownTypesRegistry"/></summary>
    public static readonly KnownTypesRegistry Default = new();

    private readonly Dictionary<string, Type> _nameToType = new();
    private readonly Dictionary<Type, string> _typeToName = new();

    private volatile IReadOnlyDictionary<string, Type>? _frozenNameToType;
    private volatile IReadOnlyDictionary<Type, string>? _frozenTypeToName;
    private volatile IReadOnlyList<Type>? _frozenKnownTypes;
    private readonly IKnownTypeAliasExtractor _extractor;

    private readonly object _lock = new();

    /// <summary>
    /// Creates new instance of <see cref="KnownTypesRegistry"/> with
    /// default <see cref="IKnownTypeAliasExtractor"/>.
    /// </summary>
    public KnownTypesRegistry(): this(DefaultKnownTypeAliasExtractor.Instance) { }

    /// <summary>
    /// Creates new instance of <see cref="KnownTypesRegistry"/> with given
    /// <see cref="IKnownTypeAliasExtractor"/>.
    /// </summary>
    /// <param name="extractor">Alias extractor.</param>
    public KnownTypesRegistry(IKnownTypeAliasExtractor extractor) =>
        _extractor = extractor;

    private IReadOnlyDictionary<K, V> Freeze<K, V>(Dictionary<K, V> source) where K: notnull
    {
        lock (_lock) return source.Freeze();
    }

    private IReadOnlyDictionary<string, Type> FrozenNameToType() =>
        // ReSharper disable once NonAtomicCompoundOperator
        _frozenNameToType ??= Freeze(_nameToType);

    private IReadOnlyDictionary<Type, string> FrozenTypeToName() =>
        // ReSharper disable once NonAtomicCompoundOperator
        _frozenTypeToName ??= Freeze(_typeToName);

    private IReadOnlyList<Type> FrozenKnownTypes() =>
        // ReSharper disable once NonAtomicCompoundOperator
        _frozenKnownTypes ??= FrozenTypeToName().Keys.ToImmutableArray();

    private Type? TryResolve(string name) =>
        FrozenNameToType().GetValueOrDefault(name);

    private string? TryResolve(Type type) =>
        FrozenTypeToName().GetValueOrDefault(type);

    private void TryRegisterImpl(string name, Type type)
    {
        // one type may have many names, but name may point to only one type
        if (_nameToType.TryGetValue(name, out var existingType))
        {
            if (existingType != type)
                throw new ArgumentException(
                    $"Cannot register {type.Name}, {existingType.Name} is already using '{name}'");
            // otherwise it is all fine!
        }
        else
        {
            _nameToType.Add(name, type);
            Thread.MemoryBarrier();
            _frozenNameToType = null;
        }

        if (!_typeToName.TryAdd(type, name))
            return;

        Thread.MemoryBarrier();
        _frozenTypeToName = null;
        _frozenKnownTypes = null;
    }

    /// <inheritdoc />
    public void Register(Type type, string alias)
    {
        lock (_lock)
        {
            TryRegisterImpl(alias, type);
        }
    }

    /// <inheritdoc />
    public void Register(Type type, IEnumerable<string>? aliases)
    {
        lock (_lock)
        {
            foreach (var name in EnumerateAliases(type, aliases))
                TryRegisterImpl(name, type);
        }
    }

    /// <inheritdoc />
    public void Register(
        IEnumerable<Type> types, Func<Type, IEnumerable<string>?> aliases)
    {
        lock (_lock)
        {
            foreach (var type in types)
            foreach (var name in EnumerateAliases(type, aliases))
                TryRegisterImpl(name, type);
        }
    }

    /// <inheritdoc />
    public void Register(
        Assembly assembly, Func<Type, bool> predicate, Func<Type, IEnumerable<string>?> aliases)
    {
        lock (_lock)
        {
            foreach (var type in assembly.GetTypes().Where(predicate))
            foreach (var name in EnumerateAliases(type, aliases))
                TryRegisterImpl(name, type);
        }
    }

    private static IEnumerable<string> EnumerateAliases(
        Type type, IEnumerable<string>? aliases) =>
        aliases ?? throw new ArgumentException(
            $"Type {type.Name} does not have any known aliases");

    private static IEnumerable<string> EnumerateAliases(
        Type type, Func<Type, IEnumerable<string>?> aliases) =>
        EnumerateAliases(type, aliases(type));

    /// <inheritdoc />
    public void Register(Type type)
    {
        Register(type, _extractor.GetAliases(type));
    }

    /// <inheritdoc />
    public void RegisterAssembly(Assembly assembly)
    {
        Register(assembly, _extractor.AutoRegister, t => _extractor.GetAliases(t));
    }

    /// <inheritdoc />
    public Type? TryGetType(string alias) => TryResolve(alias);

    /// <inheritdoc />
    public string? TryGetAlias(Type type) => TryResolve(type);

    /// <inheritdoc />
    public IReadOnlyList<Type> KnownTypes => FrozenKnownTypes();
}
