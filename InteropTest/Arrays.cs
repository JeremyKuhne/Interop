// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Runtime.InteropServices;

namespace InteropTest;

public class Arrays
{
    [DllImport(Libraries.NativeLibrary, EntryPoint = "Sum")]
    private static extern int SumNoAttributes(int[] values, int length);

    [Fact]
    public void Sum()
    {
        // The aray will be pinned, so the first value will change to 42.

        int[] values = new int[] { 1, 2, 3, 4 };
        int result = SumNoAttributes(values, values.Length);
        Assert.Equal(10, result);
        Assert.Equal(42, values[0]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "Sum")]
    private static extern int SumInAttributes([In] int[] values, int length);

    [Fact]
    public void SumIn()
    {
        // [In] makes NO difference. The array is still pinned, so the values will be changed.

        int[] values = new int[] { 1, 2, 3, 4 };
        int result = SumInAttributes(values, values.Length);
        Assert.Equal(10, result);
        Assert.Equal(42, values[0]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "Sum")]
    private static extern int SumOutAttributes([Out] int[] values, int length);

    [Fact]
    public void SumOut()
    {
        // [Out] makes NO difference. The array is still pinned, so the values will be changed.

        int[] values = new int[] { 1, 2, 3, 4 };
        int result = SumOutAttributes(values, values.Length);
        Assert.Equal(10, result);
        Assert.Equal(42, values[0]);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "IntsPointer")]
    private static extern IntPtr IntsPointer(int[] values);

    [Fact]
    public unsafe void IntsArrayDoesNotCopy()
    {
        // Another way to see that int[] pins

        int[] values = new int[1];
        fixed (void* i = values)
        {
            Assert.Equal((IntPtr)i, IntsPointer(values));
        }
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "IntsPointer")]
    private static extern IntPtr IntsPointerOut([Out] int[] values);

    [Fact]
    public unsafe void IntsArrayOutDoesNotCopy()
    {
        // Again, [Out] makes no difference

        int[] values = new int[1];
        fixed (void* i = values)
        {
            Assert.Equal((IntPtr)i, IntsPointerOut(values));
        }
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "PointsPointer")]
    private static extern IntPtr PointsPointer(Point[] points);

    [Fact]
    public unsafe void PointsArrayCopies()
    {
        // Arrays of non-primitive types copy

        Point[] points = new Point[1];
        fixed (void* p = points)
        {
            Assert.NotEqual((IntPtr)p, PointsPointer(points));
        }
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "PointsPointer")]
    private static extern IntPtr PointsPointerOut([Out] Point[] points);

    [Fact]
    public unsafe void PointsArrayOutCopies()
    {
        // [Out] makes no difference, it still copies

        Point[] points = new Point[1];
        fixed (void* p = points)
        {
            Assert.NotEqual((IntPtr)p, PointsPointer(points));
        }
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "PointsPointer")]
    private static extern IntPtr PointsPointerInOut([In, Out] Point[] points);

    [Fact]
    public unsafe void PointsArrayInOutCopies()
    {
        // [In, Out] makes no difference, it still copies

        Point[] points = new Point[1];
        fixed (void* p = points)
        {
            Assert.NotEqual((IntPtr)p, PointsPointerInOut(points));
        }
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "FlipPointers")]
    private unsafe static extern void FlipIntPointers(int** first, int** second);

    [Fact()]
    public unsafe void StopAlready()
    {
        // Doesn't corrupt state, but doesn't work. Native code
        // recieves a copy of the data.

        int[] first = new int[] { 1, 2 };
        int[] second = new int[] { 3, 4 };

        fixed (int* f = first)
        fixed (int* s = second)
        {
            FlipIntPointers(&f, &s);
        }

        Assert.Equal(new int[] { 1, 2 }, first);
        Assert.Equal(new int[] { 3, 4 }, second);
    }

    [DllImport(Libraries.NativeLibrary, EntryPoint = "FlipPointers")]
    private static extern void FlipIntArrays(ref int[] first, ref int[] second);

    [Fact(Skip = "Corrupts state.")]
    public unsafe void NowYoureJustBeingRude()
    {
        // This actually creates a copy of the array data. Trying to set the pointers on
        // the native side will give you corrupted arrays on this side. In Debug the
        // runtime will crash.

        int[] first = new int[] { 1, 2 };
        int[] second = new int[] { 3, 4 };

        FlipIntArrays(ref first, ref second);
        Assert.Equal(new int[] { 1, 2 }, second);
        Assert.Equal(new int[] { 3, 4 }, first);
    }
}
