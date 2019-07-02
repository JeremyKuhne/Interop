// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class Blittable
    {
        [Fact]
        public void BoolIsNotBlittable()
        {
            Assert.StartsWith("Object contains non-primitive or non-blittable data.",
                Assert.Throws<ArgumentException>(() => GCHandle.Alloc(new StructWithBool(), GCHandleType.Pinned)).Message);
        }

        [StructLayout(LayoutKind.Sequential)] // Suppresses CS0649 warning, but otherwise not necessary
        private struct StructWithBool
        {
            // Attributing won't change blittability
            // [MarshalAs(UnmanagedType.Bool)]
            public bool Bool;
        }

        [Fact]
        public void CharIsBlittableWithEncoding()
        {
            GCHandle.Alloc(new StructWithCharAndEncoding(), GCHandleType.Pinned).Free();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StructWithCharAndEncoding
        {
            public char Char;
        }

        [Fact]
        public void CharIsNotBlittableWithoutEncoding()
        {
            Assert.StartsWith("Object contains non-primitive or non-blittable data.",
                Assert.Throws<ArgumentException>(() => GCHandle.Alloc(new StructWithCharAndNoEncoding(), GCHandleType.Pinned)).Message);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StructWithCharAndNoEncoding
        {
            // The default marshalling here is ANSI, which isn't blittable.
            // While you can make this blittable with [MarshalAs(UnmanagedType.U2)],
            // you'll be adding unnecessary metadata overhead.
            public char Char;
        }
    }
}
