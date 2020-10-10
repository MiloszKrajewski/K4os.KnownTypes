using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Serialization;

namespace K4os.Json.KnownTypes
{
	/// <inheritdoc />
	/// <summary>
	/// Json.NET serialization binder using list of well-known type names
	/// to avoid usage of fully qualified type names for polymorphic types.
	/// </summary>
	/// <seealso cref="T:Newtonsoft.Json.Serialization.ISerializationBinder" />
	public class KnownTypesSerializationBinder: ISerializationBinder
	{
		private static readonly DefaultSerializationBinder Fallback =
			new DefaultSerializationBinder();

		/// <summary>The default <see cref="KnownTypesSerializationBinder"/></summary>
		public static readonly KnownTypesSerializationBinder Default =
			new KnownTypesSerializationBinder(new DefaultSerializationBinder());

		private readonly Dictionary<string, Type> _nameToType = new Dictionary<string, Type>();
		private readonly Dictionary<Type, string> _typeToName = new Dictionary<Type, string>();

		// reader-writer lock for bindings (many reader, one writer)
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		// Chained binder, used as fallback
		private readonly ISerializationBinder _parentBinder;

		private Type TryResolve(string name)
		{
			_lock.EnterReadLock();
			try
			{
				_nameToType.TryGetValue(name, out var type);
				return type;
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}

		private string TryResolve(Type type)
		{
			_lock.EnterReadLock();
			try
			{
				_typeToName.TryGetValue(type, out var name);
				return name;
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}

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
			}

			if (!_typeToName.ContainsKey(type))
				_typeToName.Add(type, name);
		}

		/// <summary>Registers known polymorphic type under specified name. 
		/// Expectes <see cref="JsonKnownTypeAttribute"/> annotation on a type.</summary>
		/// <typeparam name="T">Annotated known type.</typeparam>
		public void Register<T>() => Register(typeof(T));

		/// <summary>Registers known polymorphic type under specified name. 
		/// Expectes <see cref="JsonKnownTypeAttribute"/> annotation on a type.</summary>
		/// <param name="type">The known polymorphic type.</param>
		public void Register(Type type) => Register(type.GetTypeInfo());

		/// <summary>Registers known polymorphic type under specified name. 
		/// Expectes <see cref="JsonKnownTypeAttribute"/> annotation on a type.</summary>
		/// <param name="typeInfo">The known polymorphic type.</param>
		public void Register(TypeInfo typeInfo)
		{
			var names = JsonKnownTypeAttribute.EnumerateNames(typeInfo).ToArray();
			if (names.Length == 0) names = new[] { typeInfo.Name };
			var type = typeInfo.AsType();

			_lock.EnterWriteLock();
			try
			{
				foreach (var name in names)
					TryRegisterImpl(name, type);
			}
			finally
			{
				_lock.ExitWriteLock();
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
			var types = assembly
				.DefinedTypes
				.Where(ti => ti.GetCustomAttributes<JsonKnownTypeAttribute>().Any())
				.ToArray();

			foreach (var type in types)
				Register(type);
		}

		/// <summary>Registers known polymorphic type under specified name.</summary>
		/// <param name="name">The JSON friendly name.</param>
		/// <param name="type">The known polymorphic type.</param>
		public void Register(string name, Type type)
		{
			_lock.EnterWriteLock();
			try
			{
				TryRegisterImpl(name, type);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KnownTypesSerializationBinder"/> class.
		/// Used for JSON friendly serialization of polymorphic types.
		/// </summary>
		/// <param name="parentBinder">The parent binder.</param>
		public KnownTypesSerializationBinder(ISerializationBinder parentBinder = null) =>
			_parentBinder = parentBinder;

		/// <summary>
		/// When overridden in a derived class, controls the binding of a serialized object to a type.
		/// </summary>
		/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
		/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
		/// <returns>The type of the object the formatter creates a new instance of.</returns>
		public Type BindToType(string assemblyName, string typeName) =>
			(string.IsNullOrEmpty(assemblyName) ? TryResolve(typeName) : null)
			?? (_parentBinder ?? Fallback).BindToType(assemblyName, typeName);

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

			var foundName = TryResolve(serializedType);

			if (foundName != null)
			{
				typeName = foundName;
			}
			else
			{
				(_parentBinder ?? Fallback)
					.BindToName(serializedType, out assemblyName, out typeName);
			}
		}
	}
}
