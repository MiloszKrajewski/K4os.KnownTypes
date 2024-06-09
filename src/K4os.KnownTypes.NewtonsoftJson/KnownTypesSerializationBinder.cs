using System;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace K4os.KnownTypes.NewtonsoftJson;

/// <inheritdoc />
/// <summary>
/// Json.NET serialization binder using list of well-known type names
/// to avoid usage of fully qualified type names for polymorphic types.
/// </summary>
/// <seealso cref="T:Newtonsoft.Json.Serialization.ISerializationBinder" />
public class KnownTypesSerializationBinder: ISerializationBinder
{
	private static readonly DefaultSerializationBinder Fallback = new();

	/// <summary>The default <see cref="KnownTypesSerializationBinder"/></summary>
	public static readonly KnownTypesSerializationBinder Default = new();

	private readonly ISerializationBinder _parentBinder;
	private readonly KnownTypesRegistry _aliasRegistry;

	/// <summary>
	/// Initializes a new instance of the <see cref="KnownTypesSerializationBinder"/> class.
	/// Used for JSON friendly serialization of polymorphic types.
	/// </summary>
	/// <param name="aliasRegistry">Known types registry.</param>
	/// <param name="parentBinder">The parent binder.</param>
	public KnownTypesSerializationBinder(
		KnownTypesRegistry? aliasRegistry = null,
		ISerializationBinder? parentBinder = null)
	{
		_aliasRegistry = aliasRegistry ?? KnownTypesRegistry.Default;
		_parentBinder = parentBinder ?? Fallback;
	}
	
	/// <summary>Type alias registry. If you used default one you can add new ones.</summary>
	public KnownTypesRegistry Registry => _aliasRegistry;

	/// <summary>
	/// When overridden in a derived class, controls the binding of a serialized object to a type.
	/// </summary>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
	/// <returns>The type of the object the formatter creates a new instance of.</returns>
	public Type BindToType(string? assemblyName, string typeName) =>
		(string.IsNullOrEmpty(assemblyName) ? _aliasRegistry.TryGetType(typeName) : null) ?? 
		_parentBinder.BindToType(assemblyName, typeName);

	/// <summary>
	/// When overridden in a derived class, controls the binding of a serialized object to a type.
	/// </summary>
	/// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
	public void BindToName(
		Type serializedType, out string? assemblyName, out string? typeName)
	{
		assemblyName = typeName = null;

		var foundName = _aliasRegistry.TryGetAlias(serializedType);

		if (foundName != null)
		{
			typeName = foundName;
		}
		else
		{
			_parentBinder.BindToName(serializedType, out assemblyName, out typeName);
		}
	}
}