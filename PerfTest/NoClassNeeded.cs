// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PerfTest;

/// <summary>
///  C++ classes lay out the same as structs. See point.h in the NativeLibrary.
/// </summary>
public class NoClassNeeded
{
    private readonly Point[] s_points = new Point[] { new(1, 2), new(3, 4) };

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapPoints(Point* points, int count);

    [Benchmark(Baseline = true)]
    public unsafe Point[] SwapPointsStruct()
    {
        fixed (Point* p = s_points)
        {
            SwapPoints(p, s_points.Length);
        }
        return s_points;
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapPoints(IntPtr points, int count);

    [Benchmark()]
    public unsafe Point[] SwapPointsClass()
    {
        // This is similar to what System.Drawing used to do for transforming points
        // (but slightly more efficient, believe it or not)

        int size = Marshal.SizeOf<PointClass>();
        IntPtr buffer = Marshal.AllocHGlobal(size * s_points.Length);

        IntPtr current = buffer;
        foreach (Point point in s_points)
        {
            PointClass pointClass = new(point.X, point.Y);
            Marshal.StructureToPtr(pointClass, current, fDeleteOld: false);
            current = IntPtr.Add(current, size);
        }

        SwapPoints(buffer, s_points.Length);

        current = buffer;
        for (int i = 0; i < s_points.Length; i++)
        {
            PointClass pointClass = new();
            Marshal.PtrToStructure(current, pointClass);
            s_points[i] = new Point(pointClass.X, pointClass.Y);
            current = IntPtr.Add(current, size);
        }

        Marshal.FreeHGlobal(buffer);
        return s_points;
    }

    // |           Method |       Mean |     Error |    StdDev | Ratio | RatioSD |
    // |----------------- |-----------:|----------:|----------:|------:|--------:|
    // | SwapPointsStruct |   5.626 ns | 0.0672 ns | 0.0628 ns |  1.00 |    0.00 |
    // |  SwapPointsClass | 170.943 ns | 2.0232 ns | 1.8925 ns | 30.39 |    0.53 | 
}
