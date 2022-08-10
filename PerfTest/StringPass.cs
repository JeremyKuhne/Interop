// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest;

/// <summary>
///  Pinning explicitly can save a small amount of overhead as it can eliminate
///  the need for a stack frame to be created.
/// </summary>
public class StringPass
{
    static class MarshalPin
    {
        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode)]
        public static extern IntPtr StringPass(string value, int length);
    }

    [Benchmark(Baseline = true)]
    public IntPtr MarshalString()
    {
        string foo = "foo";
        return MarshalPin.StringPass(foo, foo.Length);
    }

    static class ManualPin
    {
        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode)]
        public static unsafe extern IntPtr StringPass(char* value, int length);
    }

    [Benchmark]
    public unsafe IntPtr PinString()
    {
        string foo = "foo";

        fixed (char* c = foo)
        {
            return ManualPin.StringPass(c, foo.Length);
        }
    }

    static class AllNative
    {
        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode)]
        public static unsafe extern void* StringPass(char* value, int length);
    }

    // Unfortunately cannot benchmark returning a void*

    [Benchmark]
    public unsafe IntPtr PinStringVoid()
    {
        string foo = "foo";

        fixed (char* c = foo)
        {
            return (IntPtr)AllNative.StringPass(c, foo.Length);
        }
    }

    // |          Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
    // |---------------- |---------:|----------:|----------:|------:|--------:|
    // |   MarshalString | 5.952 ns | 0.0622 ns | 0.0519 ns |  1.00 |    0.00 |
    // |       PinString | 5.559 ns | 0.0920 ns | 0.0861 ns |  0.94 |    0.02 |
    // |   PinStringVoid | 5.494 ns | 0.0952 ns | 0.0891 ns |  0.92 |    0.02 |
}
