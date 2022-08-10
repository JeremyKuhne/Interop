// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Text;

namespace InteropTest;

public class StringBuilders
{
    [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int CopyString(string source, StringBuilder destination, int destinationLength);

    [Fact]
    public void LengthAfterInvoke()
    {
        // After an interop call, the StringBuilder length is set to before the first null.
        StringBuilder builder = new(capacity: 100);
        _ = CopyString("Whizzle", builder, builder.Capacity);
        string result = builder.ToString();
        Assert.Equal("Whizzle", result);
    }

    [Fact]
    public void ConstructStringFromSpan()
    {
        // When replacing a StringBuilder with a span it is important to realize that
        // creating a string from a Span does NOT terminate at null, unlike the interop
        // call with a StringBuilder.

        string input = "Foo\0Bar";
        string output = input.AsSpan().ToString();
        Assert.Equal(input, output);
    }
}
