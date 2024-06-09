using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace K4os.KnownTypes;

/// <summary>
/// Know types registry storing list of well-known type aliases to, for example,
/// avoid usage of fully qualified type names.
/// </summary>
public class KnownTypesRegistry
{
    /// <summary>The default <see cref="KnownTypesRegistry"/></summary>
    public static readonly KnownTypesRegistry Default = new();

    private readonly Dictionary<string, Type> _nameToType = new();
    private readonly Dictionary<Type, string> _typeToName = new();

    private volatile IReadOnlyDictionary<string, Type>? _frozenNameToType;
    private volatile IReadOnlyDictionary<Type, string>? _frozenTypeToName;
    private volatile IReadOnlyList<Type>? _frozenKnownTypes;

    private readonly object _lock = new();

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

    /// <summary>Registers known polymorphic type under specified name.
    /// Type needs to be annotated with <see cref="KnownTypeAliasAttribute"/></summary>
    /// <typeparam name="T">Annotated known type.</typeparam>
    public void Register<T>() => Register(typeof(T));

    /// <summary>Registers known polymorphic type under specified name.
    /// Type needs to be annotated with <see cref="KnownTypeAliasAttribute"/></summary>
    /// <param name="type">The known polymorphic type.</param>
    public void Register(Type type) => Register(type.GetTypeInfo());

    /// <summary>Registers known polymorphic type under specified name.
    /// Type needs to be annotated with <see cref="KnownTypeAliasAttribute"/></summary>
    /// <param name="typeInfo">The known polymorphic type.</param>
    public void Register(TypeInfo typeInfo)
    {
        var names = KnownTypeAliasAttribute.EnumerateNames(typeInfo) switch {
            null or { Length: 0 } => [typeInfo.FullName.ThrowIfNull("TypeInfo.FullName")], 
            var list => list
        };
        var type = typeInfo.AsType();

        lock (_lock)
        {
            foreach (var name in names)
                TryRegisterImpl(name, type);
        }
    }

    /// <summary>Registers all types from assembly containing give type.</summary>
    /// <typeparam name="T">Hook type.</typeparam>
    public void RegisterAssembly<T>() => RegisterAssembly(typeof(T));

    /// <summary>Registers all types from assembly containing give type.</summary>
    /// <param name="hookType">Hook type.</param>
    public void RegisterAssembly(Type hookType) => RegisterAssembly(hookType.GetTypeInfo());

    /// <summary>Registers all types from assembly containing give type.</summary>
    /// <param name="hookType">Hook type.</param>
    public void RegisterAssembly(TypeInfo hookType) => RegisterAssembly(hookType.Assembly);

    /// <summary>Register all types in assembly with <see cref="KnownTypeAliasAttribute"/> annotation.</summary>
    /// <param name="assembly">Assembly.</param>
    public void RegisterAssembly(Assembly assembly)
    {
        var types = assembly
            .DefinedTypes
            .Where(ti => ti.GetCustomAttributes<KnownTypeAliasAttribute>().Any())
            .ToArray();

        foreach (var type in types)
            Register(type);
    }

    /// <summary>Registers known polymorphic type under specified name.</summary>
    /// <param name="name">The JSON friendly name.</param>
    /// <param name="type">The known polymorphic type.</param>
    public void Register(string name, Type type)
    {
        lock (_lock)
        {
            TryRegisterImpl(name, type);
        }
    }

    /// <summary>Returns type associated with given name.</summary>
    /// <param name="alias">Type alias.</param>
    /// <returns>Type associated with alias.</returns>
    public Type? TryGetType(string alias) => TryResolve(alias);

    /// <summary>Returns first alias associated with given type.</summary>
    /// <param name="type">Type.</param>
    /// <returns>First alias associated with type.</returns>
    public string? TryGetAlias(Type type) => TryResolve(type);
    
    /// <summary>
    /// Lists all known types.
    /// Please note, this method is relatively slow as list itself is not cached.
    /// </summary>
    public IReadOnlyList<Type> KnownTypes => FrozenKnownTypes();
}
