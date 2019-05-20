// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

// Some portions (as noted):
// Licensed to the .NET Foundation under one or more agreements.

using System;
using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;

namespace PerfTest
{
    [MemoryDiagnoser]
    public class GetEnvironment
    {
        // https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-getenvironmentvariable
        //
        // DWORD GetEnvironmentVariable(
        //   LPCTSTR lpName,
        //   LPTSTR  lpBuffer,
        //   DWORD   nSize
        // );

        private static class Desktop
        {
            // Copy of .NET Framework 4.7.2 implementation
            [DllImport(Libraries.Kernel32, CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = true)]
            internal static extern int GetEnvironmentVariable(string lpName, [Out]StringBuilder lpValue, int size);
        }

        private const int ERROR_ENVVAR_NOT_FOUND = 0xCB;

        [Params("USERNAME", "PATH")]
        public string Variable { get; set; }

        // Copy of .NET Framework 4.7.2 implementation
        [Benchmark(Baseline = true)]
        public string GetDesktop()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            StringBuilder blob = StringBuilderCache.Acquire(128); // A somewhat reasonable default size
            int requiredSize = Desktop.GetEnvironmentVariable(variable, blob, blob.Capacity);

            if (requiredSize == 0)
            {
                //  GetEnvironmentVariable failed
                if (Marshal.GetLastWin32Error() == ERROR_ENVVAR_NOT_FOUND)
                {
                    StringBuilderCache.Release(blob);
                    return null;
                }
            }

            while (requiredSize > blob.Capacity)
            {
                // need to retry since the environment variable might be changed 
                blob.Capacity = requiredSize;
                blob.Length = 0;
                requiredSize = Desktop.GetEnvironmentVariable(variable, blob, blob.Capacity);
            }
            return StringBuilderCache.GetStringAndRelease(blob);
        }

        // Copy of .NET Framework 4.7.2 implementation without StringBuilder cache
        [Benchmark]
        public string GetDesktopNoCache()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            StringBuilder blob = new StringBuilder(128); // A somewhat reasonable default size
            int requiredSize = Desktop.GetEnvironmentVariable(variable, blob, blob.Capacity);

            if (requiredSize == 0)
            {
                //  GetEnvironmentVariable failed
                if (Marshal.GetLastWin32Error() == ERROR_ENVVAR_NOT_FOUND)
                {
                    StringBuilderCache.Release(blob);
                    return null;
                }
            }

            while (requiredSize > blob.Capacity)
            {
                // need to retry since the environment variable might be changed 
                blob.Capacity = requiredSize;
                blob.Length = 0;
                requiredSize = Desktop.GetEnvironmentVariable(variable, blob, blob.Capacity);
            }
            return blob.ToString();
        }

        // Copy of CoreCLR code
        private static class CoreCLR
        {
            internal static unsafe int GetEnvironmentVariable(string lpName, Span<char> buffer)
            {
                fixed (char* bufferPtr = &MemoryMarshal.GetReference(buffer))
                {
                    return GetEnvironmentVariable(lpName, bufferPtr, buffer.Length);
                }
            }

            [DllImport(Libraries.Kernel32, EntryPoint = "GetEnvironmentVariableW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern unsafe int GetEnvironmentVariable(string lpName, char* lpBuffer, int nSize);
        }

        // Copy of CoreCLR code
        [Benchmark]
        public string GetCore()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            Span<char> buffer = stackalloc char[128]; // a somewhat reasonable default size
            int requiredSize = CoreCLR.GetEnvironmentVariable(variable, buffer);

            if (requiredSize == 0 && Marshal.GetLastWin32Error() == ERROR_ENVVAR_NOT_FOUND)
            {
                return null;
            }

            if (requiredSize <= buffer.Length)
            {
                return new string(buffer.Slice(0, requiredSize));
            }

            char[] chars = ArrayPool<char>.Shared.Rent(requiredSize);
            try
            {
                buffer = chars;
                requiredSize = CoreCLR.GetEnvironmentVariable(variable, buffer);
                if ((requiredSize == 0 && Marshal.GetLastWin32Error() == ERROR_ENVVAR_NOT_FOUND) ||
                    requiredSize > buffer.Length)
                {
                    return null;
                }

                return new string(buffer.Slice(0, requiredSize));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(chars);
            }
        }

        // CoreCLR code fixed and using ValueStringBuilder
        //
        // The original code doesn't handle values changing (as desktop does). Environment variables
        // are process wide, and can change from one call to the next.
        [Benchmark]
        public unsafe string GetCoreFixed()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            Span<char> stack = stackalloc char[128];
            ValueStringBuilder buffer = new ValueStringBuilder(stack);

            int returnValue;
            while ((returnValue = CoreCLR.GetEnvironmentVariable(variable, buffer.RawChars)) > buffer.Capacity)
            {
                buffer.EnsureCapacity(returnValue);
            }

            if (returnValue == 0)
            {
                return null;
            }

            buffer.Length = returnValue;
            return buffer.ToString();
        }

        [Benchmark]
        public unsafe string GetCoreFixedInline()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            Span<char> stack = stackalloc char[128];
            ValueStringBuilder buffer = new ValueStringBuilder(stack);

            int returnValue;

            while (true)
            {
                fixed (char* b = buffer)
                {
                    if ((returnValue = CoreCLR.GetEnvironmentVariable(variable, b, buffer.Capacity))
                        <= buffer.Capacity)
                    {
                        break;
                    }
                    else
                    {
                        buffer.EnsureCapacity(returnValue);
                    }
                }
            };

            if (returnValue == 0)
                return null;

            buffer.Length = returnValue;
            return buffer.ToString();
        }

        // Doing it with the minimum amount of work in the marshaller
        private static class Raw
        {
            internal static unsafe uint GetEnvironmentVariable(string name, Span<char> buffer)
            {
                fixed (char* b = buffer)
                fixed (char* n = name)
                {
                    return GetEnvironmentVariableW(n, b, (uint)buffer.Length);
                }
            }

            [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, ExactSpelling = true)]
            internal unsafe static extern uint GetEnvironmentVariableW(
                char* lpName,
                char* lpBuffer,
                uint nSize);
        }

        [Benchmark]
        public unsafe string GetRaw()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            Span<char> stack = stackalloc char[128];
            ValueStringBuilder buffer = new ValueStringBuilder(stack);

            uint returnValue;
            while ((returnValue = Raw.GetEnvironmentVariable(variable, buffer.RawChars)) > buffer.Capacity)
            {
                buffer.EnsureCapacity((int)returnValue);
            }

            if (returnValue == 0)
                return null;

            buffer.Length = (int)returnValue;
            return buffer.ToString();
        }

        [Benchmark]
        public unsafe string GetRawInline()
        {
            string variable = Variable;

            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            Span<char> stack = stackalloc char[128];
            ValueStringBuilder buffer = new ValueStringBuilder(stack);

            uint returnValue;
            fixed (char* v = variable)
            {
                while (true)
                {
                    fixed (char* b = buffer)
                    {
                        if ((returnValue = Raw.GetEnvironmentVariableW(v, b, (uint)buffer.Capacity))
                            <= buffer.Capacity)
                        {
                            break;
                        }
                        else
                        {
                            buffer.EnsureCapacity((int)returnValue);
                        }
                    }
                };
            }

            if (returnValue == 0)
                return null;

            buffer.Length = (int)returnValue;
            return buffer.ToString();
        }

        // |             Method | Variable |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        // |------------------- |--------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
        // |         GetDesktop |     PATH | 606.19 ns | 11.225 ns | 10.500 ns |  1.00 |    0.00 |      0.5112 |      0.0420 |           - |              3208 B |
        // |  GetDesktopNoCache |     PATH | 598.17 ns | 11.910 ns | 11.141 ns |  0.99 |    0.01 |      0.5112 |      0.0420 |           - |              3208 B |
        // |            GetCore |     PATH | 309.04 ns |  5.423 ns |  5.072 ns |  0.51 |    0.01 |      0.2294 |      0.0176 |           - |              1440 B |
        // |       GetCoreFixed |     PATH | 309.45 ns |  5.638 ns |  5.274 ns |  0.51 |    0.01 |      0.2294 |      0.0176 |           - |              1440 B |
        // | GetCoreFixedInline |     PATH | 313.43 ns |  6.021 ns |  6.183 ns |  0.52 |    0.01 |      0.2294 |      0.0176 |           - |              1440 B |
        // |             GetRaw |     PATH | 294.46 ns |  2.401 ns |  2.246 ns |  0.49 |    0.01 |      0.2294 |      0.0176 |           - |              1440 B |
        // |       GetRawInline |     PATH | 304.26 ns |  3.563 ns |  3.333 ns |  0.50 |    0.01 |      0.2294 |      0.0176 |           - |              1440 B |
        // |                    |          |           |           |           |       |         |             |             |             |                     |
        // |         GetDesktop | USERNAME |  78.18 ns |  1.136 ns |  1.062 ns |  1.00 |    0.00 |      0.0050 |           - |           - |                32 B |
        // |  GetDesktopNoCache | USERNAME |  96.88 ns |  1.618 ns |  1.513 ns |  1.24 |    0.03 |      0.0573 |      0.0001 |           - |               360 B |
        // |            GetCore | USERNAME |  67.36 ns |  1.377 ns |  1.288 ns |  0.86 |    0.02 |      0.0050 |           - |           - |                32 B |
        // |       GetCoreFixed | USERNAME |  70.38 ns |  1.482 ns |  1.386 ns |  0.90 |    0.02 |      0.0050 |           - |           - |                32 B |
        // | GetCoreFixedInline | USERNAME |  70.77 ns |  1.207 ns |  1.129 ns |  0.91 |    0.02 |      0.0050 |           - |           - |                32 B |
        // |             GetRaw | USERNAME |  65.37 ns |  1.344 ns |  1.320 ns |  0.84 |    0.02 |      0.0050 |           - |           - |                32 B |
        // |       GetRawInline | USERNAME |  68.44 ns |  1.334 ns |  1.183 ns |  0.87 |    0.02 |      0.0050 |           - |           - |                32 B |

        // Perf Takeaways:
        //
        // 1. Skipping all marshalling is fastest
        // 2. Go for simpler logic (helper that pins)
    }
}
