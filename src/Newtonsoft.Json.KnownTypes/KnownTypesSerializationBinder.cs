using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.KnownTypes
{
	/// <summary>
	/// Json.NET serialization binder using list of well-known type names
	/// to avoid usage of fully qualified type names for polymorphic types.
	/// </summary>
	/// <seealso cref="ISerializationBinder" />
	public class KnownTypesSerializationBinder: ISerializationBinder
	{
		/// <summary>The default <see cref="KnownTypesSerializationBinder"/></summary>
		public static readonly KnownTypesSerializationBinder Default =
			new KnownTypesSerializationBinder(new DefaultSerializationBinder());

		private class Binding
		{
			public string Name { get; set; }
			public Type Type { get; set; }
		}

		// NOTE: you might be tempted to use dictionary instead of list
		// for now, this dictionary is so small (few entries) that performance
		// difference if negligible
		// once we reach ~20 entries we can change it to two (!) dictionaries
		// see: https://i.stack.imgur.com/O4ly9.png
		private readonly IList<Binding> _knownBindings = new List<Binding>();

		// Chained binder, used as fallback
		private readonly ISerializationBinder _parentBinder;

		private Binding TryResolve(string name)
		{
			lock (_knownBindings)
				return _knownBindings.FirstOrDefault(p => p.Name == name);
		}

		private Binding TryResolve(Type type)
		{
			lock (_knownBindings)
				return _knownBindings.FirstOrDefault(p => p.Type == type);
		}

		/// <summary>Registers known polymorphic type under specified name. 
		/// Expectes <see cref="JsonKnownTypeAttribute"/> annotation on a type.</summary>
		/// <typeparam name="T">Annotated known type.</typeparam>
		public void Register<T>() => Register(typeof(T));

		/// <summary>Registers known polymorphic type under specified name. 
		/// Expectes <see cref="JsonKnownTypeAttribute"/> annotation on a type.</summary>
		/// <param name="type">The known polymorphic type.</param>
		public void Register(Type type)
		{
			lock (_knownBindings)
			{
				var names = JsonKnownTypeAttribute.EnumerateNames(type).ToArray();
				if (names.Length == 0) names = new[] { type.Name };
				foreach (var name in names)
				{
					_knownBindings.Add(new Binding { Name = name, Type = type });
				}
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

		/// <summary>Register all types in assembly with <see cref="JsonKnownTypeAttribute"/> annotation.</summary>
		/// <param name="assembly">Assembly.</param>
		public void RegisterAssembly(Assembly assembly)
		{
			var types = assembly.DefinedTypes
				.Where(ti => ti.GetCustomAttributes<JsonKnownTypeAttribute>().Any())
				.ToArray();

			foreach (var type in types)
			{
				Register(type.AsType());
			}
		}

		/// <summary>Registers known polymorphic type under specified name.</summary>
		/// <param name="name">The JSON friendly name.</param>
		/// <param name="type">The known polymorphic type.</param>
		public void Register(string name, Type type)
		{
			lock (_knownBindings)
				_knownBindings.Add(new Binding { Name = name, Type = type });
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KnownTypesSerializationBinder"/> class.
		/// Used for JSON friendly serialization of polymorphic types.
		/// </summary>
		/// <param name="parentBinder">The parent binder.</param>
		public KnownTypesSerializationBinder(ISerializationBinder parentBinder = null)
		{
			_parentBinder = parentBinder;
		}

		/// <summary>
		/// When overridden in a derived class, controls the binding of a serialized object to a type.
		/// </summary>
		/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
		/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
		/// <returns>The type of the object the formatter creates a new instance of.</returns>
		public Type BindToType(string assemblyName, string typeName) =>
			(string.IsNullOrEmpty(assemblyName) ? TryResolve(typeName)?.Type : null)
			?? _parentBinder?.BindToType(assemblyName, typeName);

		/// <summary>
		/// When overridden in a derived class, controls the binding of a serialized object to a type.
		/// </summary>
		/// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
		/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
		/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
		public void BindToName(
			Type serializedType, out string assemblyName, out string typeName)
		{
			assemblyName = typeName = null;

			var found = TryResolve(serializedType);

			if (found != null)
			{
				typeName = found.Name;
			}
			else
			{
				_parentBinder?.BindToName(serializedType, out assemblyName, out typeName);
			}
		}
	}
}
