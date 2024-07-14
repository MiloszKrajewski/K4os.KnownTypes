namespace K4os.KnownTypes;

/// <summary>
/// Interface for extracting type aliases from types.
/// </summary>
public interface IKnownTypeAliasExtractor
{
    /// <summary>Determines if given type should be automatically registered when scanning
    /// assemblies.</summary>
    /// <param name="type">Type if question.</param>
    /// <returns><c>true</c> if type should be registered when scanning assembly;
    /// <c>false</c> otherwise.</returns>
    bool AutoRegister(Type type);

    /// <summary>Gets aliases for the given type.</summary>
    /// <param name="type">Type in question.</param>
    /// <returns>List of aliases.</returns>
    string[]? GetAliases(Type type);
}
