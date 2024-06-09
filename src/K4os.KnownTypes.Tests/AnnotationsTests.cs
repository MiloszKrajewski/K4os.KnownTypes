using Xunit;

namespace K4os.KnownTypes.Tests;

public class AnnotationsTests
{
    [KnownTypeAlias("a0"), KnownTypeAlias("a1"), KnownTypeAlias("a2")]
    class A;

    [KnownTypeAlias("b")]
    class B;

    class C;
    
    [Fact]
    public void AttributeCanBeUsedToRegister()
    {
        var registry = new KnownTypesRegistry();
        registry.Register<A>();
        registry.Register<B>();
        
        Assert.Equal("a0", registry.TryGetAlias(typeof(A)));
        Assert.Equal("b", registry.TryGetAlias(typeof(B)));
        
        Assert.Equal(typeof(A), registry.TryGetType("a0"));
        Assert.Equal(typeof(A), registry.TryGetType("a1"));
        Assert.Equal(typeof(A), registry.TryGetType("a2"));
        
        Assert.Equal(typeof(B), registry.TryGetType("b"));
    }
    
    [Fact]
    public void WhenTypesAreNotKnownItReturnsNull()
    {
        var registry = new KnownTypesRegistry();
        registry.Register<A>();
        registry.Register<B>();
        
        Assert.Equal("a0", registry.TryGetAlias(typeof(A)));
        Assert.Equal("b", registry.TryGetAlias(typeof(B)));
        Assert.Null(registry.TryGetAlias(typeof(C)));
        
        Assert.Equal(typeof(A), registry.TryGetType("a0"));
        Assert.Equal(typeof(A), registry.TryGetType("a1"));
        Assert.Equal(typeof(A), registry.TryGetType("a2"));
        Assert.Null(registry.TryGetType("a3"));
        Assert.Null(registry.TryGetType("c"));
        
        Assert.Equal(typeof(B), registry.TryGetType("b"));
    }
    
    [Fact]
    public void TypeWithoutAliasIsRegisteredWithFullName()
    {
        var registry = new KnownTypesRegistry();
        registry.Register<C>();
        
        var fullName = typeof(C).FullName.ThrowIfNull();
        
        Assert.Equal(fullName, registry.TryGetAlias(typeof(C)));
        Assert.Equal(typeof(C), registry.TryGetType(fullName));
    }
}
