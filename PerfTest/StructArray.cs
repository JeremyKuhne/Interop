// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PerfTest
{
    public class StructArray
    {
        private Point[] _points = new Point[] { new Point(1, 2), new Point(3, 4) };

        [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
        private static extern void SwapByRef(ref Point points, int count);

        [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
        private unsafe static extern void SwapByPointer(Point* points, int count);

        [Benchmark(Baseline = true)]
        public void SwapPointsByRef() => SwapByRef(ref _points[0], _points.Length);

        [Benchmark]
        public unsafe void SwapPointsByPointer()
        {
            fixed (Point* p = _points)
            {
                SwapByPointer(p, _points.Length);
            }
        }

        // |              Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |
        // |-------------------- |---------:|----------:|----------:|---------:|------:|--------:|
        // |     SwapPointsByRef | 7.903 ns | 0.1857 ns | 0.2064 ns | 7.899 ns |  1.00 |    0.00 |
        // | SwapPointsByPointer | 6.721 ns | 0.1624 ns | 0.4133 ns | 6.544 ns |  0.92 |    0.05 |
    }
}
