// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PerfTest;

public class ViaDelegate
{
    // Native methods can also be called by pointer by creating a delegate around
    // the pointer with Marshal.GetDelegateForFunctionPointer. Calling in this way
    // is slower than doing a standard P/Invoke.
    //
    // C# plans to add better support for function pointers in the future, which
    // will provide another way to invoke native entry points.
    //
    // https://github.com/dotnet/csharplang/blob/master/proposals/function-pointers.md

    internal const string NativeLibrary = "NativeLibrary.dll";

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int DoubleDelegate(int value);

    private DoubleDelegate? Doubler;

    [GlobalSetup]
    public void Setup()
    {
        IntPtr handle = LoadLibraryExW(NativeLibrary, IntPtr.Zero, LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH);
        Doubler = GetFunctionDelegate<DoubleDelegate>(handle, "Double");
    }

    [Benchmark]
    public int CallViaDelegate() => Doubler!(1999);

    [DllImport(Libraries.NativeLibrary)]
    private static extern int Double(int value);

    [Benchmark(Baseline = true)]
    public int CallViaPInvoke() => Double(1999);

    // |          Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
    // |---------------- |----------:|----------:|----------:|------:|--------:|
    // | CallViaDelegate | 10.374 ns | 0.1879 ns | 0.1758 ns |  2.23 |    0.08 |
    // |  CallViaPInvoke |  4.649 ns | 0.1439 ns | 0.1346 ns |  1.00 |    0.00 |

    public static DelegateType GetFunctionDelegate<DelegateType>(IntPtr libraryHandle, string methodName)
    {
        IntPtr method = GetProcAddress(libraryHandle, methodName);
        if (method == IntPtr.Zero)
            throw new Win32Exception();

        return Marshal.GetDelegateForFunctionPointer<DelegateType>(method);
    }

    // This API is only available in ANSI (which is the default CharSet)
    [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true, BestFitMapping = false, CharSet = CharSet.Ansi)]
    public static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string methodName);

    [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    public static extern IntPtr LoadLibraryExW(
        string lpFileName,
        IntPtr hFile,
        LoadLibraryFlags dwFlags);

    [DllImport(Libraries.Kernel32, SetLastError = true)]
    public static extern bool FreeLibrary(
        IntPtr hModule);

    [Flags]
    public enum LoadLibraryFlags : uint
    {
        DONT_RESOLVE_DLL_REFERENCES         = 0x00000001,
        LOAD_LIBRARY_AS_DATAFILE            = 0x00000002,
        LOAD_WITH_ALTERED_SEARCH_PATH       = 0x00000008,
        LOAD_IGNORE_CODE_AUTHZ_LEVEL        = 0x00000010,
        LOAD_LIBRARY_AS_IMAGE_RESOURCE      = 0x00000020,
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE  = 0x00000040,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR    = 0x00000100,
        LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
        LOAD_LIBRARY_SEARCH_USER_DIRS       = 0x00000400,
        LOAD_LIBRARY_SEARCH_SYSTEM32        = 0x00000800,
        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS    = 0x00001000
    }
}
