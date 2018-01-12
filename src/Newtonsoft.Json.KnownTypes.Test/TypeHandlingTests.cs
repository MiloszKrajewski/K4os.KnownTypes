using Xunit;

namespace Newtonsoft.Json.KnownTypes.Test
{
	public class TypeHandlingTests
	{
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
	}
}
