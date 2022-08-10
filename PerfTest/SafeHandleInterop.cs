// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest;

/// <summary>
///  SafeHandle is safe, but comes at a cost. Consider using IntPtr where
///  performance is critical and risk of not closing a handle is low. Another
///  alternative is to include wrap handle use in a CriticalFinalizerObject.
///  See <see cref="System.IO.Enumeration.FileSystemEnumerator{TResult}" />
///  for an example of this.
/// </summary>
public class SafeHandleInterop
{
    public class MySafeHandle : SafeHandle
    {
        public MySafeHandle()
            : base (IntPtr.Zero, ownsHandle: true)
        { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            VoidPass(handle);
            return true;
        }

        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern void VoidPass(IntPtr handle);
    }

    static class AsSafeHandle
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern MySafeHandle VoidReturn();
    }

    [Benchmark(Baseline = true)]
    public IntPtr UseSafeHandle()
    {
        using MySafeHandle handle = AsSafeHandle.VoidReturn();
        return handle.DangerousGetHandle();
    }

    static class AsIntPtr
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern IntPtr VoidReturn();

        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern void VoidPass(IntPtr handle);
    }

    [Benchmark()]
    public IntPtr UseIntPtr()
    {
        IntPtr handle = AsIntPtr.VoidReturn();
        try
        {
            return IntPtr.Add(handle, 1);
        }
        finally
        {
            AsIntPtr.VoidPass(handle);
        }
    }

    static class AsHandleRef
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern IntPtr VoidReturn();

        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public static extern void VoidPass(HandleRef handle);
    }

    [Benchmark()]
    public IntPtr UseHandleRef()
    {
        IntPtr handle = AsHandleRef.VoidReturn();
        try
        {
            return IntPtr.Add(handle, 1);
        }
        finally
        {
            // This is a misuse of HandleRef. "this" will never get collected. We have plenty of code that does this
            // unfortunately.
            AsHandleRef.VoidPass(new HandleRef(this, handle));
        }
    }

    // |        Method |     Mean |     Error |    StdDev | Ratio |
    // |-------------- |---------:|----------:|----------:|------:|
    // | UseSafeHandle | 72.14 ns | 1.4311 ns | 1.4055 ns |  1.00 |
    // |     UseIntPtr | 12.12 ns | 0.0630 ns | 0.0558 ns |  0.17 |
    // |  UseHandleRef | 14.15 ns | 0.2973 ns | 0.2781 ns |  0.20 |
}
