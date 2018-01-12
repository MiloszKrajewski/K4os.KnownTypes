using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Newtonsoft.Json.KnownTypes.Test
{
	public class JsonKnownTypeAttributeTests
	{
		private static void TestDeserialization<T>(ISerializationBinder binder, string name)
		{
			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder,
			};
			var json = $"{{\"$type\":\"{name}\"}}";
			Assert.IsType<T>(JsonConvert.DeserializeObject(json, settings));
		}

		private static void TestSerialization(ISerializationBinder binder, string name, object obj)
		{
			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder,
			};
			var json = JsonConvert.SerializeObject(obj, settings);
			var jobj = JObject.Parse(json);
			Assert.Equal(name, jobj["$type"].ToString());
		}

		[JsonKnownType("A0"), JsonKnownType("A1"), JsonKnownType("A2")]
		public class ClassA { }

		[JsonKnownType("B0")]
		public class ClassB { }

		public class ClassC { }

		[Fact]
		public void AttributeCanBeUsedToRegister()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register<ClassA>();
			binder.Register<ClassB>();

			TestSerialization(binder, "A0", new ClassA());
			TestDeserialization<ClassA>(binder, "A0");
			TestSerialization(binder, "B0", new ClassB());
			TestDeserialization<ClassB>(binder, "B0");
		}

		[Fact]
		public void FirstAttributeTakesPriority()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register<ClassA>();
			binder.Register<ClassB>();

			TestSerialization(binder, "A0", new ClassA());
			TestDeserialization<ClassA>(binder, "A0");
			TestDeserialization<ClassA>(binder, "A1");
			TestDeserialization<ClassA>(binder, "A2");
		}

		[Fact]
		public void RegisterAssemblyRegistersAllAnnotatedTypes()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.RegisterAssembly(GetType().GetTypeInfo().Assembly);

			TestSerialization(binder, "A0", new ClassA());
			TestDeserialization<ClassA>(binder, "A0");
			TestDeserialization<ClassA>(binder, "A1");
			TestDeserialization<ClassA>(binder, "A2");

			TestSerialization(binder, "B0", new ClassB());
			TestDeserialization<ClassB>(binder, "B0");

			var fallbackClassC =
				"Newtonsoft.Json.KnownTypes.Test.JsonKnownTypeAttributeTests+ClassC, " +
				"Newtonsoft.Json.KnownTypes.Test";

			TestSerialization(binder, fallbackClassC, new ClassC());
		}
	}
}
