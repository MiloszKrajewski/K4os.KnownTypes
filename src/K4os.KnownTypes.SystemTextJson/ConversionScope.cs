namespace K4os.KnownTypes.SystemTextJson;

/// <summary>
/// Specifies the scope of <c>JsonElement</c> to object conversion.
/// </summary>
[Flags]
public enum ConversionScope
{
    /// <summary>Only known object types.</summary>
    KnownObjects = 0x0001,

    /// <summary>Dictionary types.</summary>
    Dictionaries = 0x0002,

    /// <summary>Array types.</summary>
    Arrays = 0x0004,

    /// <summary>Boolean values.</summary>
    Booleans = 0x0008,

    /// <summary>Integer values.</summary>
    Integers = 0x0010,

    /// <summary>Floating point values.</summary>
    Floats = 0x0020,

    /// <summary>String values.</summary>
    Strings = 0x0040,

    /// <summary>GUID values.</summary>
    Guids = 0x0080,

    /// <summary>Timestamp values.</summary>
    Timestamps = 0x0100,

    /// <summary>Null values.</summary>
    Nulls = 0x0200,

    /// <summary>Default: objects, dictionaries, arrays, booleans, floats, strings, nulls.</summary>
    Default = KnownObjects | Dictionaries | Arrays | Booleans | Floats | Strings | Nulls,

    /// <summary>Full: default plus integers, GUIDs, and timestamps.</summary>
    Full = Default | Integers | Guids | Timestamps
}
