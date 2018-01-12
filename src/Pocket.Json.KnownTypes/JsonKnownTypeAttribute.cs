using System;
using System.Linq;
using System.Reflection;

namespace Pocket.Json.KnownTypes
{
	/// <inheritdoc />
	/// <summary>
	/// Attrbiute to be put on well-known serializable type to be used with
	/// <see cref="KnownTypesSerializationBinder"/> and JsonSerializer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class JsonKnownTypeAttribute: Attribute
	{
		/// <summary>The well-known type name.</summary>
		/// <value>The name.</value>
		public string Name { get; }

		/// <inheritdoc />
		/// <summary>Creates association between annotated type and given name.</summary>
		/// <param name="name">Associated name.</param>
		public JsonKnownTypeAttribute(string name) => Name = name;

		/// <summary>Enumerates names associated with given type.</summary>
		/// <param name="type">Type in question.</param>
		/// <returns>Sequence of associated names.</returns>
		public static string[] EnumerateNames(TypeInfo type) =>
			type.GetCustomAttributes(typeof(JsonKnownTypeAttribute), false)
				.OfType<JsonKnownTypeAttribute>()
				.Select(a => a.Name)
				.ToArray();
	}
}
