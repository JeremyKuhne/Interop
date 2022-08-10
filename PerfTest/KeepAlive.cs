// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest;

/// <summary>
///  HandleRef is essentially a way to force GC.KeepAlive (presuming you use it correctly).
///  It comes with a slight cost.
/// </summary>
public class KeepAlive
{
    static class AsIntPtr
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern IntPtr VoidReturn();

        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern void VoidPass(IntPtr handle);
    }

    static class AsHandleRef
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern void VoidPass(HandleRef handle);
    }

    private static readonly HandleWrapper s_wrapper = new((IntPtr)0xDADADADA);

    [Benchmark(Baseline = true)]
    public IntPtr UseHandleRefTypical()
    {
        // This is a bit closer to the normal usage of HandleRef. Some managed
        // class is wrapping a native handle and we don't want to lose it before
        // the method returns.
        AsHandleRef.VoidPass(new HandleRef(s_wrapper, s_wrapper.Handle));
        return s_wrapper.Handle;
    }

    [Benchmark()]
    public IntPtr UseGCKeepAlive()
    {
        // Effectively Handleref is doing a GC.KeepAlive.
        AsIntPtr.VoidPass(s_wrapper.Handle);
        GC.KeepAlive(s_wrapper);
        return s_wrapper.Handle;
    }

    public class HandleWrapper : IDisposable
    {
        public HandleWrapper(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; private set; }

        ~HandleWrapper()
        {
            Dispose();
        }

        public void Dispose()
        {
            // Imagine this is a native handle cleanup routine
            AsIntPtr.VoidPass(Handle);
        }
    }

    // |              Method |     Mean |     Error |    StdDev | Ratio |
    // |-------------------- |---------:|----------:|----------:|------:|
    // | UseHandleRefTypical | 7.237 ns | 0.0287 ns | 0.0268 ns |  1.00 |
    // |      UseGCKeepAlive | 5.557 ns | 0.1135 ns | 0.1062 ns |  0.77 |
}
