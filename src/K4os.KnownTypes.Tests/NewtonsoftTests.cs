using System.Reflection;
using K4os.KnownTypes.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace K4os.KnownTypes.Tests;

public class NewtonsoftTests
{
    private static (KnownTypesRegistry Registry, KnownTypesSerializationBinder Binder) CreateBinder()
    {
        var registry = new KnownTypesRegistry();
        var binder = new KnownTypesSerializationBinder(registry);
        return (registry, binder);
    }

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
        var (registry, binder) = CreateBinder();
        registry.Register<ClassA>();
        registry.Register<ClassB>();

        TestSerialization(binder, "A0", new ClassA());
        TestDeserialization<ClassA>(binder, "A0");
        TestSerialization(binder, "B0", new ClassB());
        TestDeserialization<ClassB>(binder, "B0");
    }

    [Fact]
    public void FirstAttributeTakesPriority()
    {
        var (registry, binder) = CreateBinder();
        registry.Register<ClassA>();
        registry.Register<ClassB>();

        TestSerialization(binder, "A0", new ClassA());
        TestDeserialization<ClassA>(binder, "A0");
        TestDeserialization<ClassA>(binder, "A1");
        TestDeserialization<ClassA>(binder, "A2");
    }

    [Fact]
    public void RegisterAssemblyRegistersAllAnnotatedTypes()
    {
        var (registry, binder) = CreateBinder();
        registry.RegisterAssembly(GetType().GetTypeInfo().Assembly);

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
        var (registry, binder) = CreateBinder();
        registry.Register("envl", typeof(Envelope));
        registry.Register("base", typeof(Base));
        registry.Register("drvd", typeof(Derived));

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
        var (registry1, binder1) = CreateBinder();
        registry1.Register("envl", typeof(Envelope));
        registry1.Register("base", typeof(Base));
        registry1.Register("drvd", typeof(Derived));

        var (registry2, binder2) = CreateBinder();
        registry2.Register("envl", typeof(Envelope));
        registry2.Register("base", typeof(Base));
        registry2.Register("drvd", typeof(Other));

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
        var (registry, binder) = CreateBinder();
        registry.Register("aname", typeof(Base));
        registry.Register("bname", typeof(Derived));
        registry.Register("cname", typeof(Other));

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
        var (registry, binder) = CreateBinder();
        registry.Register("aname", typeof(Base));
        registry.Register("bname", typeof(Derived));
        registry.Register("cname", typeof(Derived));

        TestDeserialization<Base>(binder, "aname");
        TestDeserialization<Derived>(binder, "bname");
        TestDeserialization<Derived>(binder, "cname");

        TestSerialization(binder, "aname", new Base());
        TestSerialization(binder, "bname", new Derived());
    }

    [Fact]
    public void WithManyNamesFirstOneIsUsed()
    {
        var (registry, binder) = CreateBinder();
        registry.Register("aname", typeof(Derived));
        registry.Register("bname", typeof(Derived));
        registry.Register("cname", typeof(Derived));

        TestDeserialization<Derived>(binder, "aname");
        TestDeserialization<Derived>(binder, "bname");
        TestDeserialization<Derived>(binder, "cname");

        TestSerialization(binder, "aname", new Derived());
    }
}
