using DemoApp;
using Xunit;

namespace DemoApp.Tests;

public class DemoTests
{
    [Fact]
    public void Add_ReturnsCorrectSum()
    {
        var demo = new Demo();
        int result = demo.Add(2, 3);
        Assert.Equal(5, result);
    }
}
