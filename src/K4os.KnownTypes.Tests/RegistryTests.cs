using System.Text;
using Xunit;

namespace K4os.KnownTypes.Tests;

public class RegistryTests
{
    [Fact]
    public void SameNameCanBeRegisteredTwiceAsLongTypeIsTheSame()
    {
        var registry = new KnownTypesRegistry();
        registry.Register(typeof(Base), "aname");
        registry.Register(typeof(Base), "aname");
        registry.Register(typeof(Base), "aname");
    }

    [Fact]
    public void SameNameCannotBeRegisteredTwice()
    {
        var registry = new KnownTypesRegistry();
        registry.Register(typeof(Base), "aname");
        registry.Register(typeof(Derived), "bname");
        Assert.Throws<ArgumentException>(() => registry.Register(typeof(Other), "bname"));
    }
    
    [Fact]
    public void WhenTypeIsNotKnownItReturnsNull()
    {
        var registry = new KnownTypesRegistry();
        Assert.Null(registry.TryGetType("unknown"));
        Assert.Null(registry.TryGetAlias(typeof(StringBuilder)));
    }
}
