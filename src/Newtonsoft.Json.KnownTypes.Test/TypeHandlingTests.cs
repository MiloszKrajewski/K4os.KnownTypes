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

		public class Envelope
		{
			public Base Data { get; set; }
		}

		[Fact]
		public void NoTypeHandling()
		{
			string Serialize(Envelope e) => JsonConvert.SerializeObject(e);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var json = Serialize(envelope);

			Assert.NotEqual(json, Serialize(Deserialize(json)));
		}

		[Fact]
		public void AllTypesHandling()
		{
			var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
			string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings);
			Envelope Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings);

			var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

			var json = Serialize(envelope);

			Assert.Equal(json, Serialize(Deserialize(json)));
		}

		[Fact]
		public void KnownTypeHandling()
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
	}
}
