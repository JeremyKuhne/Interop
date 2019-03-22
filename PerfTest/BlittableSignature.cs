// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest
{
    public class BlittableSignature
    {
        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern bool NaiveInvert(bool value);

        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern BOOL BlittableInvert(BOOL value);

        // In Windows headers, BOOL is defined as an int
        internal enum BOOL : int
        {
            FALSE = 0,
            TRUE = 1,
        }

        [Benchmark(Baseline = true)]
        public bool InvertTrueNaive() => NaiveInvert(true);

        [Benchmark()]
        public bool InvertFalseNaive() => NaiveInvert(false);

        [Benchmark()]
        public bool InvertTrueEnum() => BlittableInvert(BOOL.TRUE) != BOOL.FALSE;

        [Benchmark()]
        public bool InvertFalseEnum() => BlittableInvert(BOOL.FALSE) != BOOL.FALSE;

        // |           Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |
        // |----------------- |---------:|----------:|----------:|---------:|------:|--------:|
        // |  InvertTrueNaive | 5.340 ns | 0.0821 ns | 0.0728 ns | 5.345 ns |  1.00 |    0.00 |
        // | InvertFalseNaive | 5.334 ns | 0.1221 ns | 0.1142 ns | 5.370 ns |  1.00 |    0.03 |
        // |   InvertTrueEnum | 4.772 ns | 0.0986 ns | 0.0874 ns | 4.784 ns |  0.89 |    0.01 |
        // |  InvertFalseEnum | 3.816 ns | 0.1547 ns | 0.3706 ns | 3.684 ns |  0.82 |    0.09 |
    }
}
