using System.Reflection;
using K4os.KnownTypes.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace K4os.KnownTypes.Tests;

public class NewtonsoftTests
{
    private static KnownTypesSerializationBinder CreateBinder() => new(new KnownTypesRegistry());

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
        Assert.Equal(name, jobj["$type"]!.ToString());
    }

    [Fact]
    public void AttributeCanBeUsedToRegister()
    {
        var binder = CreateBinder();
        binder.Registry.Register<ClassA>();
        binder.Registry.Register<ClassB>();

        TestSerialization(binder, "A0", new ClassA());
        TestDeserialization<ClassA>(binder, "A0");
        TestSerialization(binder, "B0", new ClassB());
        TestDeserialization<ClassB>(binder, "B0");
    }

    [Fact]
    public void FirstAttributeTakesPriority()
    {
        var binder = CreateBinder();
        binder.Registry.Register<ClassA>();
        binder.Registry.Register<ClassB>();

        TestSerialization(binder, "A0", new ClassA());
        TestDeserialization<ClassA>(binder, "A0");
        TestDeserialization<ClassA>(binder, "A1");
        TestDeserialization<ClassA>(binder, "A2");
    }

    [Fact]
    public void RegisterAssemblyRegistersAllAnnotatedTypes()
    {
        var binder = CreateBinder();
        binder.Registry.RegisterAssembly(GetType().GetTypeInfo().Assembly);

        TestSerialization(binder, "A0", new ClassA());
        TestDeserialization<ClassA>(binder, "A0");
        TestDeserialization<ClassA>(binder, "A1");
        TestDeserialization<ClassA>(binder, "A2");

        TestSerialization(binder, "B0", new ClassB());
        TestDeserialization<ClassB>(binder, "B0");

        var fallbackClassC =
            "K4os.KnownTypes.Tests.ClassC, K4os.KnownTypes.Tests";

        TestSerialization(binder, fallbackClassC, new ClassC());
    }

    [Fact]
    public void SupportsPolymorphism()
    {
        var binder = CreateBinder();
        binder.Registry.Register("envl", typeof(Envelope));
        binder.Registry.Register("base", typeof(Base));
        binder.Registry.Register("drvd", typeof(Derived));

        var settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All,
            SerializationBinder = binder,
        };

        string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings);
        Envelope? Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings);

        var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };

        var serialized = Serialize(envelope);
        var deserialized = Deserialize(serialized);

        Assert.IsType<Derived>(deserialized!.Data);
        Assert.Equal(7.0, ((Derived)deserialized.Data).Value);
        Assert.Equal(serialized, Serialize(deserialized));
    }

    [Fact]
    public void NameCanRedirectToDifferentType()
    {
        var binder1 = CreateBinder();
        binder1.Registry.Register("envl", typeof(Envelope));
        binder1.Registry.Register("base", typeof(Base));
        binder1.Registry.Register("drvd", typeof(Derived));

        var binder2 = CreateBinder();
        binder2.Registry.Register("envl", typeof(Envelope));
        binder2.Registry.Register("base", typeof(Base));
        binder2.Registry.Register("drvd", typeof(Other));

        var settings1 = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All,
            SerializationBinder = binder1,
        };
        var settings2 = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All,
            SerializationBinder = binder2,
        };

        string Serialize(Envelope e) => JsonConvert.SerializeObject(e, settings1);
        Envelope? Deserialize(string j) => JsonConvert.DeserializeObject<Envelope>(j, settings2);

        var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };
        var roundtrip = Deserialize(Serialize(envelope));

        Assert.True(roundtrip!.Data is Other);
        Assert.Equal("derived", roundtrip.Data.Text);
        Assert.Equal(7.0, ((Other)roundtrip.Data).Value);
    }

    [Fact]
    public void CanRegisterManyTypes()
    {
        var binder = CreateBinder();
        binder.Registry.Register("aname", typeof(Base));
        binder.Registry.Register("bname", typeof(Derived));
        binder.Registry.Register("cname", typeof(Other));

        TestDeserialization<Base>(binder, "aname");
        TestDeserialization<Derived>(binder, "bname");
        TestDeserialization<Other>(binder, "cname");

        TestSerialization(binder, "aname", new Base());
        TestSerialization(binder, "bname", new Derived());
        TestSerialization(binder, "cname", new Other());
    }

    [Fact]
    public void CanRegisterSameTypeWithManyNames()
    {
        var binder = CreateBinder();
        binder.Registry.Register("aname", typeof(Base));
        binder.Registry.Register("bname", typeof(Derived));
        binder.Registry.Register("cname", typeof(Derived));

        TestDeserialization<Base>(binder, "aname");
        TestDeserialization<Derived>(binder, "bname");
        TestDeserialization<Derived>(binder, "cname");

        TestSerialization(binder, "aname", new Base());
        TestSerialization(binder, "bname", new Derived());
    }

    [Fact]
    public void WithManyNamesFirstOneIsUsed()
    {
        var binder = CreateBinder();
        binder.Registry.Register("aname", typeof(Derived));
        binder.Registry.Register("bname", typeof(Derived));
        binder.Registry.Register("cname", typeof(Derived));

        TestDeserialization<Derived>(binder, "aname");
        TestDeserialization<Derived>(binder, "bname");
        TestDeserialization<Derived>(binder, "cname");

        TestSerialization(binder, "aname", new Derived());
    }
}
