// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class ByReference
    {
        [DllImport(Libraries.NativeLibrary, EntryPoint = "DoubleByRef")]
        private static extern IntPtr Double(ref int value);

        [Fact]
        public void Basic()
        {
            int value = 11;
            Double(ref value);
            Assert.Equal(22, value);
        }

        [Fact]
        public unsafe void RefPassesDirectly()
        {
            int value = 12;
            IntPtr localAddress = (IntPtr)(void*)&value;
            IntPtr address = Double(ref value);
            Assert.Equal(24, value);
            Assert.Equal(localAddress, address);
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "DoubleByRef")]
        private static extern IntPtr DoubleOut(out int value);

        [Fact]
        public unsafe void OutPassesDirectly()
        {
            int value = 15;
            IntPtr localAddress = (IntPtr)(void*)&value;
            IntPtr address = DoubleOut(out value);
            Assert.Equal(30, value);
            Assert.Equal(localAddress, address);
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "DoubleByRef")]
        private static unsafe extern void* DoublePointer(int* value);

        [Fact]
        public unsafe void DoubleUnsafe()
        {
            int value = 15;
            IntPtr localAddress = (IntPtr)(void*)&value;
            void* address = DoublePointer(&value);
            Assert.Equal(30, value);
            Assert.Equal(localAddress, (IntPtr)address);
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "InvertByRef")]
        private static extern IntPtr Invert(ref bool value);

        [Fact]
        public unsafe void NonBlittableByRef()
        {
            // "bool" is not directly blittable and will introduce extra
            // marshalling code, slowing down the call.

            bool value = true;
            IntPtr localAddress = (IntPtr)(void*)&value;
            IntPtr address = Invert(ref value);
            Assert.False(value);

            // The address the native method gets is a temporary scratch copy.
            // The marshaller converts bool to an integer, which matches BOOL.
            Assert.NotEqual(localAddress, address);
        }
    }
}
