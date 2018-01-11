using Xunit;

namespace Newtonsoft.Json.KnownTypes.Test
{
	public class TypeHandlingTests
	{
		public class Base
		{
			public string Text { get; set; }
		}

		public class Derived: Base
		{
			public int Value { get; set; }
		}

		public class Other: Base
		{
			public double Value { get; set; }
		}

		public class Envelope
		{
			public Base Data { get; set; }
		}

		[Fact]
		public void NoTypeHandlingDoesNotSupportPolymorphism()
		{
			string Serialize(Envelope e) => JsonConvert.SerializeObject(e);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var json = Serialize(envelope);

			Assert.NotEqual(json, Serialize(Deserialize(json)));
		}

		[Fact]
		public void AllTypesHandlingSupportsPolymorphism()
		{
			var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
			string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var json = Serialize(envelope);

			Assert.Equal(json, Serialize(Deserialize(json)));
		}

		[Fact]
		public void KnownTypeHandlingSupportsPolymorphism()
		{
			var binder = new KnownTypesSerializationBinder();
			binder.Register("Envelope", typeof(Envelope));
			binder.Register("Base", typeof(Base));
			binder.Register("Derived", typeof(Derived));

			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = binder,
			};

			string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var json = Serialize(envelope);

			Assert.Equal(json, Serialize(Deserialize(json)));
		}

		[Fact]
		public void NameCanRedirectToDifferentType()
		{
			var binder1 = new KnownTypesSerializationBinder();
			binder1.Register("Envelope", typeof(Envelope));
			binder1.Register("Base", typeof(Base));
			binder1.Register("Derived", typeof(Derived));

			var binder2 = new KnownTypesSerializationBinder();
			binder2.Register("Envelope", typeof(Envelope));
			binder2.Register("Base", typeof(Base));
			binder2.Register("Derived", typeof(Other));

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

		// cannot register name twice
		// can give multiple names to one type
		// first name is used for serialization
	}
}
