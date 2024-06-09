using System;

namespace K4os.KnownTypes.Tests;

public class Base
{
    public string? Text { get; set; }
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
    public Base? Data { get; set; }
}

[KnownTypeAlias("A0"), KnownTypeAlias("A1"), KnownTypeAlias("A2")]
public class ClassA;

[KnownTypeAlias("B0")]
public class ClassB;

public class ClassC;
