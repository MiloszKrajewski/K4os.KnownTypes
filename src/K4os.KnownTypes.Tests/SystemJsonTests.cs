using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using K4os.KnownTypes.SystemTextJson;
using Xunit;

namespace K4os.KnownTypes.Tests;

public class SystemJsonTests
{
    private static JsonSerializerOptions CreateOptions(KnownTypesRegistry? registry = null) => new() {
        TypeInfoResolver = new KnownTypesJsonTypeInfoResolver(registry ?? new KnownTypesRegistry())
    };
    
    private static void TestDeserialization<T>(KnownTypesRegistry registry, string name) => 
        TestDeserialization<T>(CreateOptions(registry), name);

    private static void TestDeserialization<T>(JsonSerializerOptions options, string name)
    {
        var json = $"{{\"$type\":\"{name}\"}}";
        Assert.IsType<T>(JsonSerializer.Deserialize<T>(json, options));
    }
    
    private static void TestSerialization(KnownTypesRegistry registry, string name, object obj) => 
        TestSerialization(CreateOptions(registry), name, obj);

    private static void TestSerialization(JsonSerializerOptions options, string name, object obj)
    {
        var json = JsonSerializer.Serialize(obj, options);
        var jobj = JsonNode.Parse(json)!;
        Assert.Equal(name, jobj["$type"]!.ToString());
    }

    [Fact]
    public void AttributeCanBeUsedToRegister()
    {
        var registry = new KnownTypesRegistry();
        registry.Register<ClassA>();
        registry.Register<ClassB>();

        TestSerialization(registry, "A0", new ClassA());
        TestDeserialization<ClassA>(registry, "A0");
        TestSerialization(registry, "B0", new ClassB());
        TestDeserialization<ClassB>(registry, "B0");
    }

    [Fact]
    public void FirstAttributeTakesPriority()
    {
        var registry = new KnownTypesRegistry();
        registry.Register<ClassA>();
        registry.Register<ClassB>();

        TestSerialization(registry, "A0", new ClassA());
        TestDeserialization<ClassA>(registry, "A0");
        TestDeserialization<ClassA>(registry, "A1");
        TestDeserialization<ClassA>(registry, "A2");
    }

    [Fact]
    public void RegisterAssemblyRegistersAllAnnotatedTypes()
    {
        var registry = new KnownTypesRegistry();
        registry.RegisterAssembly(GetType().GetTypeInfo().Assembly);

        TestSerialization(registry, "A0", new ClassA());
        TestDeserialization<ClassA>(registry, "A0");
        TestDeserialization<ClassA>(registry, "A1");
        TestDeserialization<ClassA>(registry, "A2");

        TestSerialization(registry, "B0", new ClassB());
        TestDeserialization<ClassB>(registry, "B0");
        
        // there is no fallback for ClassC as it is for Newtonsoft.Json
    }

    [Fact]
    public void SupportsPolymorphism()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("envl", typeof(Envelope));
        registry.Register("base", typeof(Base));
        registry.Register("drvd", typeof(Derived));

        var options = CreateOptions(registry);

        string Serialize(Envelope e) => JsonSerializer.Serialize(e, options);
        Envelope? Deserialize(string j) => JsonSerializer.Deserialize<Envelope>(j, options);

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
        var registry1 = new KnownTypesRegistry();
        registry1.Register("envl", typeof(Envelope));
        registry1.Register("base", typeof(Base));
        registry1.Register("drvd", typeof(Derived));

        var registry2 = new KnownTypesRegistry();
        registry2.Register("envl", typeof(Envelope));
        registry2.Register("base", typeof(Base));
        registry2.Register("drvd", typeof(Other));

        var settings1 = CreateOptions(registry1);
        var settings2 = CreateOptions(registry2);

        string Serialize(Envelope e) => JsonSerializer.Serialize(e, settings1);
        Envelope? Deserialize(string j) => JsonSerializer.Deserialize<Envelope>(j, settings2);

        var envelope = new Envelope { Data = new Derived { Text = "derived", Value = 7 } };
        var roundtrip = Deserialize(Serialize(envelope));

        Assert.True(roundtrip!.Data is Other);
        Assert.Equal("derived", roundtrip.Data.Text);
        Assert.Equal(7.0, ((Other)roundtrip.Data).Value);
    }

    [Fact]
    public void CanRegisterManyTypes()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Base));
        registry.Register("bname", typeof(Derived));
        registry.Register("cname", typeof(Other));

        TestDeserialization<Base>(registry, "aname");
        TestDeserialization<Derived>(registry, "bname");
        TestDeserialization<Other>(registry, "cname");

        TestSerialization(registry, "aname", new Base());
        TestSerialization(registry, "bname", new Derived());
        TestSerialization(registry, "cname", new Other());
    }

    [Fact]
    public void CanRegisterSameTypeWithManyNames()
    {
        var binder = new KnownTypesRegistry();
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
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Derived));
        registry.Register("bname", typeof(Derived));
        registry.Register("cname", typeof(Derived));

        TestDeserialization<Derived>(registry, "aname");
        TestDeserialization<Derived>(registry, "bname");
        TestDeserialization<Derived>(registry, "cname");

        TestSerialization(registry, "aname", new Derived());
    }
    
    [Fact]
    public void StillWorkWhenDeserializingAsObject()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Derived));
        
        var options = CreateOptions(registry);
        var serialized = JsonSerializer.Serialize(new Derived(), options);
        var deserialized1 = JsonSerializer.Deserialize<Base>(serialized, options);
        var deserialized2 = JsonSerializer.Deserialize<object>(serialized, options);
        var deserialized3 = JsonSerializer.Deserialize<JsonElement>(serialized, options);
        
        Assert.IsType<Derived>(deserialized1);
        Assert.IsType<JsonElement>(deserialized2);
        Assert.IsType<JsonElement>(deserialized3);
    }
    
    [Fact]
    public void WithoutObjectConverterObjectIsDeserializedAsJsonElement()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Derived));
        
        var options = CreateOptions(registry);
        var serialized = JsonSerializer.Serialize(new Derived(), options);
        var deserialized1 = JsonSerializer.Deserialize<Base>(serialized, options);
        var deserialized2 = JsonSerializer.Deserialize<object>(serialized, options);
        
        Assert.IsType<Derived>(deserialized1);
        Assert.IsType<JsonElement>(deserialized2);
    }
    
    [Fact]
    public void CanUseObjectConverterToAvoidJsonElement()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Derived));
        
        var options = CreateOptions(registry);
        options.Converters.Add(new KnownTypeObjectConverter(registry));

        var serialized = JsonSerializer.Serialize(new Derived(), options);
        var deserialized1 = JsonSerializer.Deserialize<Base>(serialized, options);
        var deserialized2 = JsonSerializer.Deserialize<object>(serialized, options);
        
        Assert.IsType<Derived>(deserialized1);
        Assert.IsType<Derived>(deserialized2);
    }
    
    [Fact]
    public void FullScopeDictionaryConversion()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("derived", typeof(Derived));

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new KnownTypesJsonTypeInfoResolver(registry)
        };
        // enable object converter with Full scope
        options.Converters.Add(new KnownTypeObjectConverter(registry, ConversionScope.Full));

        var original = new Derived { Text = "text", Value = 42 };
        // this is anonymous object with various field types, best we can get back is Dictionary<string, object?>
        var payload = new {
            numberInt = 123L,
            numberFloat = 3.14,
            booleanTrue = true,
            guid = Guid.NewGuid(),
            timestamp = DateTime.UtcNow,
            justString = "hello",
            knownObject = original,
            array = new object[] { 1L, 2.1, true, "world", original },
            justNull = (object?)null
        };

        var json = JsonSerializer.Serialize(payload, options);
        var roundtripObject = JsonSerializer.Deserialize<object>(json, options)!;
        var roundtripMap = Assert.IsType<Dictionary<string, object?>>(roundtripObject);

        // Verify types according to Full scope
        Assert.IsType<long>(roundtripMap["numberInt"]);
        Assert.IsType<double>(roundtripMap["numberFloat"]);
        Assert.IsType<bool>(roundtripMap["booleanTrue"]);
        Assert.True(roundtripMap["guid"] is Guid);
        Assert.True(roundtripMap["timestamp"] is DateTime);
        Assert.IsType<string>(roundtripMap["justString"]);
        Assert.IsType<Derived>(roundtripMap["knownObject"]);
        Assert.Null(roundtripMap["justNull"]);

        var array = Assert.IsType<object[]>(roundtripMap["array"]);
        Assert.IsType<long>(array[0]);
        Assert.IsType<double>(array[1]);
        Assert.IsType<bool>(array[2]);
        Assert.IsType<string>(array[3]);
        Assert.IsType<Derived>(array[4]);
    }
}
