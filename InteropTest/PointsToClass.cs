// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Runtime.InteropServices;

namespace InteropTest;

public class PointsToClass
{
    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoint")]
    private static extern void SwapByRef(ref Point point);

    [Fact]
    public void SwapPointByRef()
    {
        Point point = new Point(1, 2);
        SwapByRef(ref point);
        Assert.Equal(new Point(2, 1), point);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoint")]
    private static extern void SwapClass(PointClass point);

    [Fact]
    public void SwapPointClass()
    {
        PointClass point = new PointClass(1, 2);
        SwapClass(point);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private static extern void SwapByRef(ref Point points, int count);

    [Fact]
    public void SwapPointsByRef()
    {
        Span<Point> points = stackalloc Point[] { new Point(1, 2), new Point(3, 4) };
        SwapByRef(ref points[0], points.Length);

        Assert.Equal(new Point(2, 1), points[0]);
        Assert.Equal(new Point(4, 3), points[1]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapByPointer(Point* points, int count);

    [Fact]
    public unsafe void SwapPointsByPointer()
    {
        Span<Point> points = stackalloc Point[] { new Point(1, 2), new Point(3, 4) };

        fixed (Point* p = points)
        {
            SwapByPointer(p, points.Length);
        }

        Assert.Equal(new Point(2, 1), points[0]);
        Assert.Equal(new Point(4, 3), points[1]);
    }

    [Fact]
    public unsafe void PointClassMarshalsAsPoint()
    {
        Assert.Equal(sizeof(Point), Marshal.SizeOf<PointClass>());
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapByArrayNoAttribute(
        Point[] points,
        int count);

    [Fact]
    public void SwapPointsByArrayInOnly()
    {
        Point[] points = new Point[] { new Point(1, 2), new Point(3, 4) };

        SwapByArrayNoAttribute(points, points.Length);

        // .NET makes a copy and does not copy the data back by default.
        // As such our data is unchanged.

        Assert.Equal(new Point(1, 2), points[0]);
        Assert.Equal(new Point(3, 4), points[1]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapByArrayOutAttribute(
        [Out] Point[] points,
        int count);

    [Fact]
    public void SwapPointsByArrayOutOnly()
    {
        Point[] points = new Point[] { new Point(1, 2), new Point(3, 4) };

        SwapByArrayOutAttribute(points, points.Length);

        // .NET gives a zeroed out memory block when marked as out. The
        // native code flips the zeroes and they're copied back.

        Assert.Equal(new Point(0, 0), points[0]);
        Assert.Equal(new Point(0, 0), points[1]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "SwapPoints")]
    private unsafe static extern void SwapByArray(
        [In, Out] Point[] points,
        int count);

    [Fact]
    public void SwapPointsByArray()
    {
        Point[] points = new Point[] { new Point(1, 2), new Point(3, 4) };

        SwapByArray(points, points.Length);

        // Now that we're attributed correctly we'll get our expected behavior.

        Assert.Equal(new Point(2, 1), points[0]);
        Assert.Equal(new Point(4, 3), points[1]);
    }
}
