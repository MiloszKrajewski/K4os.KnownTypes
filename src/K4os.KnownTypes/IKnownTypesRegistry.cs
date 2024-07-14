using System.Reflection;

namespace K4os.KnownTypes;

/// <summary>Registry of known types.</summary>
public interface IKnownTypesRegistry
{
    /// <summary>Registers known polymorphic type under specified name.</summary>
    /// <param name="alias">Alias.</param>
    /// <param name="type">The known polymorphic type.</param>
    void Register(Type type, string alias);

    /// <summary>Registers known polymorphic type under specified name.</summary>
    /// <param name="aliases">List of aliases.</param>
    /// <param name="type">The known polymorphic type.</param>
    void Register(Type type, IEnumerable<string>? aliases);

    /// <summary>Registers known polymorphic types under specified names.</summary>
    /// <param name="types">The known polymorphic types.</param>
    /// <param name="aliases">Function returning list of aliases for given type.</param>
    void Register(
        IEnumerable<Type> types, Func<Type, IEnumerable<string>?> aliases);

    /// <summary>Registers known polymorphic types under specified names.</summary>
    /// <param name="assembly">Assembly to scan for types.</param>
    /// <param name="predicate">Predicate to filter types.</param>
    /// <param name="aliases">Function returning list of aliases for given type.</param>
    void Register(
        Assembly assembly, Func<Type, bool> predicate, Func<Type, IEnumerable<string>?> aliases);

    /// <summary>Registers known polymorphic type under specified name.</summary>
    /// <param name="type">The known polymorphic type.</param>
    void Register(Type type);
    
    /// <summary>Registers all types from the specified assembly.</summary>
    /// <param name="assembly">Assembly to scan for types.</param>
    void RegisterAssembly(Assembly assembly);
}
