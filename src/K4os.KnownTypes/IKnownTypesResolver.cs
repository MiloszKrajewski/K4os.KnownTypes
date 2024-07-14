namespace K4os.KnownTypes;

/// <summary>
/// Interface for resolving known types.
/// </summary>
public interface IKnownTypesResolver
{
    /// <summary>Returns type associated with given name.</summary>
    /// <param name="alias">Type alias.</param>
    /// <returns>Type associated with alias.</returns>
    Type? TryGetType(string alias);

    /// <summary>Returns first alias associated with given type.</summary>
    /// <param name="type">Type.</param>
    /// <returns>First alias associated with type.</returns>
    string? TryGetAlias(Type type);

    /// <summary>
    /// Lists all known types.
    /// Please note, this method is relatively slow as list itself is not cached.
    /// </summary>
    IReadOnlyList<Type> KnownTypes { get; }
}
