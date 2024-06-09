using System.Text;
using Xunit;

namespace K4os.KnownTypes.Tests;

public class RegistryTests
{
    [Fact]
    public void SameNameCanBeRegisteredTwiceAsLongTypeIsTheSame()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Base));
        registry.Register("aname", typeof(Base));
        registry.Register("aname", typeof(Base));
    }

    [Fact]
    public void SameNameCannotBeRegisteredTwice()
    {
        var registry = new KnownTypesRegistry();
        registry.Register("aname", typeof(Base));
        registry.Register("bname", typeof(Derived));
        Assert.Throws<ArgumentException>(() => registry.Register("bname", typeof(Other)));
    }
    
    [Fact]
    public void WhenTypeIsNotKnownItReturnsNull()
    {
        var registry = new KnownTypesRegistry();
        Assert.Null(registry.TryGetType("unknown"));
        Assert.Null(registry.TryGetAlias(typeof(StringBuilder)));
    }
}
