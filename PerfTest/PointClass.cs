// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace PerfTest
{
    // Essentially telling our class to layout like a struct
    [StructLayout(LayoutKind.Sequential)]
    public class PointClass
    {
        public int X;
        public int Y;

        public PointClass()
        {
        }

        public PointClass(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
