// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Emit;

namespace PerfTest;

// TODO: Generate IL stub & calli
public class DirectCall
{
    public static void Do()
    {
        var dynamicMethod = new DynamicMethod(
            "Test",
            typeof(IntPtr),
            new[] { typeof(void*), typeof(char*), typeof(int) });

        var il = dynamicMethod.GetILGenerator();
    }
}
