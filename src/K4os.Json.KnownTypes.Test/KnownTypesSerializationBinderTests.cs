using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace K4os.Json.KnownTypes.Test
{
	public class KnownTypesSerializationBinderTests
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

		[Fact]
		public void SupportsPolymorphism()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("envl", typeof(Envelope));
			binder.Register("base", typeof(Base));
			binder.Register("drvd", typeof(Derived));

			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder,
			};

			string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var serialized = Serialize(envelope);
			var deserialized = Deserialize(serialized);

			Assert.IsType<Derived>(deserialized.Data);
			Assert.Equal(7.0, ((Derived) deserialized.Data).Value);
			Assert.Equal(serialized, Serialize(deserialized));
		}

		[Fact]
		public void NameCanRedirectToDifferentType()
		{
			var binder1 = new KnownTypesSerializationBinder();
			binder1.Register("envl", typeof(Envelope));
			binder1.Register("base", typeof(Base));
			binder1.Register("drvd", typeof(Derived));

			var binder2 = new KnownTypesSerializationBinder();
			binder2.Register("envl", typeof(Envelope));
			binder2.Register("base", typeof(Base));
			binder2.Register("drvd", typeof(Other));

			var settings1 = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder1,
			};
			var settings2 = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder2,
			};

			string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings1);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings2);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };
			var roundtrip = Deserialize(Serialize(envelope));

			Assert.True(roundtrip.Data is Other);
			Assert.Equal("derived", roundtrip.Data.Text);
			Assert.Equal(7.0, ((Other) roundtrip.Data).Value);
		}

		[Fact]
		public void CanRegisterManyTypes()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("aname", typeof(Base));
			binder.Register("bname", typeof(Derived));
			binder.Register("cname", typeof(Other));

			TestDeserialization<Base>(binder, "aname");
			TestDeserialization<Derived>(binder, "bname");
			TestDeserialization<Other>(binder, "cname");

			TestSerialization(binder, "aname", new Base());
			TestSerialization(binder, "bname", new Derived());
			TestSerialization(binder, "cname", new Other());
		}

		[Fact]
		public void SameNameCannotBeRegisteredTwice()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("aname", typeof(Base));
			binder.Register("bname", typeof(Derived));

			Assert.Throws<ArgumentException>(() => binder.Register("bname", typeof(Other)));
		}

		[Fact]
		public void CanRegisterSameTypeWithManyNames()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("aname", typeof(Base));
			binder.Register("bname", typeof(Derived));
			binder.Register("cname", typeof(Derived));

			TestDeserialization<Base>(binder, "aname");
			TestDeserialization<Derived>(binder, "bname");
			TestDeserialization<Derived>(binder, "cname");

			TestSerialization(binder, "aname", new Base());
			TestSerialization(binder, "bname", new Derived());
		}

		[Fact]
		public void WithManyNamesFirstOneIsUsed()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("aname", typeof(Derived));
			binder.Register("bname", typeof(Derived));
			binder.Register("cname", typeof(Derived));

			TestDeserialization<Derived>(binder, "aname");
			TestDeserialization<Derived>(binder, "bname");
			TestDeserialization<Derived>(binder, "cname");

			TestSerialization(binder, "aname", new Derived());
		}
	}
}
