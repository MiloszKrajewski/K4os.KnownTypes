# Newtonsoft.Json.KnownTypes

![NuGet Stats](https://img.shields.io/nuget/v/Newtonsoft.Json.KnownTypes.svg)

Serialization binder allowing to assign custom names to types.

## Background

One of the frequent problems with `Newtonsoft.Json` serializer is polymorphic serialization. It is not turned on by default and when it get turned on the results are... not pretty (subjective opinion).

### Usage

Let's assume you have class hierarchy:

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

When you serialized this envelope with default serialization settings:

```csharp
var json = JsonConvert.SerializeObject(envelope);
```

generated JSON will not contain any type information:

```json
{"Data":{"Value":7,"Text":"derived"}}
```

When you deserialize this JSON:

```csharp
var data = JsonConvert.DeserializeObject<Envelope>(json);
```

you maybe surprised that the object inside envelope is just of type `Base` not `Derived`.

It is because `Base` is nominal type inside envelope no additional clues are provided.

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
    "$type":"Newtonsoft.Json.KnownTypes.Test.Envelope, Newtonsoft.Json.KnownTypes.Test",
    "Data":{
        "$type":"Newtonsoft.Json.KnownTypes.Test.Derived, Newtonsoft.Json.KnownTypes.Test",
        "Value":7,
        "Text":"derived"
    }
}
```

This is kind-of what me might want. Slightly better results can be achieved with `TypeNameHandling.Auto` which includes type information only if actual type does not match nominal type (but it may fail you if nominal type changes over time).

This solution does have one more disadvantage: **persisted message cannot be read if we move them to different assembly or change their name**. This is not ideal in constantly evolving system.

So what can we do?

We can add another level of indirection - assigning "code names" to types, and serialize just those. We can move/rename types as we like as long as code names stay assigned to right types.

What we need is to create a binder and assign names to types:

```csharp
var binder = new KnownTypesSerializationBinder();
binder.Register("Envelope", typeof(Envelope));
binder.Register("Base", typeof(Base));
binder.Register("Derived", typeof(Derived));
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
{"$type":"Envelope","Data":{"$type":"Derived","Value":7,"Text":"derived"}}
```

and not tightly coupled with actual type.

## JsonKnownTypeAttribute

## Versioning

## Build

```shell
paket restore
fake build
```
