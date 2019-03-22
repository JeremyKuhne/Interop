// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PerfTest
{
    public class NoClassNeeded
    {
        private Point[] _points = new Point[] { new Point(1, 2), new Point(3, 4) };

        [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
        private unsafe static extern void SwapPoints(Point* points, int count);

        [Benchmark]
        public unsafe void SwapPointsStruct()
        {
            fixed (Point* p = _points)
            {
                SwapPoints(p, _points.Length);
            }
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
        private unsafe static extern void SwapPoints(PointClass[] points, int count);
    }
}
