# Newtonsoft.Json.KnownTypes

![NuGet Stats](https://img.shields.io/nuget/v/Newtonsoft.Json.KnownTypes.svg)

Serialization binder allowing to assign custom names to types.

## Background

One of the frequent problems with `Newtonsoft.Json` serializer is polymorphic serialization. It is not turned on by default and when it get turned on the results are... not pretty (subjective opinion).

### Usage

Let's assume we have class hierarchy:

```csharp
class Base { public string Text; }
class Derived: Base { public int Value; }
```

and an `Envelope` class:

```csharp
class Envelope { public Base Data; }
```

Let's now create an envelope object holding an instance of `Derived` class. Please note, Envelope's declared member is `Base` but we will use `Derived` (this is ok, of course, as `Derived` is a subclass of `Base`).

```csharp
var envelope = new Envelope {
    Data = new Derived { Text = "derived", Value = 7 }
}
```

When we serialized this envelope with default serialization settings:

```csharp
var json = JsonConvert.SerializeObject(envelope);
```

generated JSON will not contain any type information:

```json
{"Data":{"Value":7,"Text":"derived"}}
```

When we deserialize this JSON:

```csharp
var data = JsonConvert.DeserializeObject<Envelope>(json);
```

the object inside envelope will be just of type `Base` not `Derived` and it just lost `Value` field. If we serialize this is again we will get `{"Data":{"Text":"derived"}}` only.

It is because `Base` is nominal (declared) type inside envelope and no additional clues are provided.

To force serializer to include type information we can use `JsonSerializerSettings`:

```csharp
var settings = new JsonSerializerSettings {
    TypeNameHandling = TypeNameHandling.All
};
var envelope = new Envelope {
    Data = new Derived { Text = "derived", Value = 7 }
};
var json = JsonConvert.SerializeObject(envelope, settings);
```

which attached type information to message, generating slightly inflated message:

```json
{
    "$type":"Some.Potentially.Very.Long.Namespace.Envelope, And.Here.Comes.Assembly.Name",
    "Data":{
        "$type":"Some.Potentially.Very.Long.Namespace.Derived, And.Here.Comes.Assembly.Name",
        "Value":7,
        "Text":"derived"
    }
}
```

This is kind-of what me might want. Slightly better results can be achieved with `TypeNameHandling.Auto` which includes type information only if actual type does not match nominal type (but it may fail us if nominal type changes over time, for example we change `Data` field type to `object` and now all previosly persisted `Base` objects are without annotation, thus no properly deserialized).

One of disadvantages is tightly coupling JSON message to .NET assembly. It supposed to be portable, right? Plase note, that we can't even move them to different assembly or change their names, as `$type` filed will no longer match. This is not ideal in constantly evolving system.

So what can we do?

We can add another level of indirection - assigning "code names" to types, and serialize just those. We can move/rename types as we like as long as code names stay assigned to appropriate types.

What we need is to create a binder and assign names to types:

```csharp
var binder = new KnownTypesSerializationBinder();
binder.Register("envelope.v1", typeof(Envelope));
binder.Register("base.v1", typeof(Base));
binder.Register("derived.v1", typeof(Derived));
```

Then we can just use this binder for serialization:

```csharp
var settings = new JsonSerializerSettings {
    TypeNameHandling = TypeNameHandling.All,
    SerializationBinder = binder
};
```

Since then type information will be much neater:

```json
{"$type":"envelope.v1","Data":{"$type":"derived.v1","Value":7,"Text":"derived"}}
```

and not tightly coupled with actual assembly nor type.


## JsonKnownTypeAttribute

## Versioning

## Build

```shell
paket restore
fake build
```
