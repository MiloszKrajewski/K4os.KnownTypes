namespace K4os.KnownTypes;

/// <summary>
/// Extension methods for <see cref="IKnownTypesRegistry"/>.
/// </summary>
public static class Extensions
{
    /// <summary>Registers a known polymorphic type under the specified name.</summary>
    /// <param name="registry">Type registry.</param>
    /// <param name="alias">Type alias.</param>
    /// <param name="type">Type to register.</param>
    public static void Register(
        this IKnownTypesRegistry registry, string alias, Type type) =>
        registry.Register(type, alias);

    /// <summary>Registers a known polymorphic type under the specified name.</summary>
    /// <param name="registry">Type registry.</param>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>(
        this IKnownTypesRegistry registry) =>
        registry.Register(typeof(T));

    /// <summary>Registers a known polymorphic type under the specified name.</summary>
    /// <param name="registry">Type registry.</param>
    /// <param name="alias">Type alias.</param>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>(
        this IKnownTypesRegistry registry, string alias) =>
        registry.Register(typeof(T), alias);

    /// <summary>Registers a known polymorphic type under the specified name.</summary>
    /// <param name="registry">Type registry.</param>
    /// <param name="aliases">List of aliases.</param>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>(
        this IKnownTypesRegistry registry, IEnumerable<string> aliases) =>
        registry.Register(typeof(T), aliases);
    
    /// <summary>Registers a known polymorphic type under the specified name.</summary>
    /// <param name="registry">Type registry.</param>
    /// <param name="aliases">List of aliases.</param>
    /// <typeparam name="T">Type to register.</typeparam>
    public static void Register<T>(
        this IKnownTypesRegistry registry, params string[] aliases) =>
        registry.Register(typeof(T), aliases);

    /// <summary>Registers all types from the specified assembly.</summary>
    /// <param name="registry">Type registry.</param>
    /// <typeparam name="THook">Type from the assembly.</typeparam>
    public static void RegisterAssembly<THook>(this IKnownTypesRegistry registry) =>
        registry.RegisterAssembly(typeof(THook));

    /// <summary>Registers all types from the specified assembly.</summary>
    /// <param name="registry">Type registry.</param>
    /// <param name="hookType">Type from the assembly.</param>
    public static void RegisterAssembly(this IKnownTypesRegistry registry, Type hookType) =>
        registry.RegisterAssembly(hookType.Assembly);
}
