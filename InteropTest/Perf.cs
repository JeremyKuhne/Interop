// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using PerfTest;

namespace InteropTest;

public class Perf
{
    [Theory,
        InlineData("USERNAME"),
        InlineData("PATH")]
    public void GetEnvironment(string value)
    {
        string? expected = Environment.GetEnvironmentVariable(value);

        var perfTest = new GetEnvironment { Variable = value };
        Assert.Equal(expected, perfTest.GetDesktop());
        Assert.Equal(expected, perfTest.GetDesktopNoCache());
        Assert.Equal(expected, perfTest.GetCore());
        Assert.Equal(expected, perfTest.GetCoreFixed());
        Assert.Equal(expected, perfTest.GetCoreFixedInline());
        Assert.Equal(expected, perfTest.GetRaw());
        Assert.Equal(expected, perfTest.GetRawInline());
    }

    [Fact]
    public void GetEnvironment_NotFound()
    {
        var perfTest = new GetEnvironment { Variable = Guid.NewGuid().ToString() };
        Assert.Null(perfTest.GetDesktop());
        Assert.Null(perfTest.GetDesktopNoCache());
        Assert.Null(perfTest.GetCore());
        Assert.Null(perfTest.GetCoreFixed());
        Assert.Null(perfTest.GetCoreFixedInline());
        Assert.Null(perfTest.GetRaw());
        Assert.Null(perfTest.GetRawInline());
    }
}
