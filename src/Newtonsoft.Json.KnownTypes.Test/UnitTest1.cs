using System;
using Xunit;
using Newtonsoft.Json.KnownTypes;

namespace Newtonsoft.Json.KnownTypes.Test
{
	class Base
	{
		public string Text;
	}

	class Derived: Base
	{
		public int Value;
	}

	class Envelope
	{
		public Base Data;
	}

	public class UnitTest1
	{
		[Fact]
		public void NoTypeHandling()
		{
			var envelope = new Envelope {
				Data = new Derived { Text = "derived", Value = 7 }
			};
			var json = JsonConvert.SerializeObject(envelope);
			var data = JsonConvert.DeserializeObject<Envelope>(json);
		}

		[Fact]
		public void AllTypesHandling()
		{
			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All
			};
			var envelope = new Envelope {
				Data = new Derived { Text = "derived", Value = 7 }
			};
			var json = JsonConvert.SerializeObject(envelope, settings);
			var data = JsonConvert.DeserializeObject<Envelope>(json, settings);
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

			var envelope = new Envelope {
				Data = new Derived { Text = "derived", Value = 7 }
			};

			var json = JsonConvert.SerializeObject(envelope, settings);
			var data = JsonConvert.DeserializeObject<Envelope>(json, settings);
		}

	}
}
