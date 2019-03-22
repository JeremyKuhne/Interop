// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class Strings
    {
        [DllImport(Libraries.NativeLibrary, EntryPoint = "StringPass", CharSet = CharSet.Unicode)]
        private static extern IntPtr StringPass(string value, int length);

        [Fact]
        public unsafe void StringPins()
        {
            string foo = "foo";
            fixed (void* s = foo)
            {
                Assert.Equal((IntPtr)s, StringPass(foo, foo.Length));
            }
        }
    }
}
