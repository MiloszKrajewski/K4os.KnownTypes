using System;
using System.Linq;
using System.Reflection;

namespace K4os.KnownTypes;

/// <summary>Attribute to be put on well-known serializable type.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class KnownTypeAliasAttribute: Attribute
{
	/// <summary>The well-known type name.</summary>
	/// <value>The name.</value>
	public string Name { get; }

	/// <inheritdoc />
	/// <summary>Creates association between annotated type and given name.</summary>
	/// <param name="name">Associated name.</param>
	public KnownTypeAliasAttribute(string name) => Name = name;

	/// <summary>Enumerates names associated with given type.</summary>
	/// <param name="type">Type in question.</param>
	/// <returns>Sequence of associated names.</returns>
	public static string[] EnumerateNames(TypeInfo type) =>
		type.GetCustomAttributes(typeof(KnownTypeAliasAttribute), false)
			.OfType<KnownTypeAliasAttribute>()
			.Select(a => a.Name)
			.ToArray();
}