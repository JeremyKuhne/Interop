// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class MatchingSignatures
    {
        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public unsafe static extern void* InvertByRef(void* value);

        [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
        public unsafe static extern void* GetMeaning(void* value);

        private static class More
        {
            [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
            public unsafe static extern void* InvertByRef(int* value);

            [DllImport(Libraries.NativeLibrary, ExactSpelling = true)]
            public unsafe static extern void* GetMeaning(void* value);
        }

        [Fact]
        public unsafe void MethodsAreShared()
        {
            MethodsAreSharedInternal();
            MethodsAreSharedInternal();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe void MethodsAreSharedInternal()
        {
            // In the disassembly you can see that the calls for the two methods below
            // jump to a location that loads the actual MethodDesc and then jumps (first)
            // to the stub generation code, then the second call through replaces both
            // with the same generated code for every method that matches.

            MethodInfo one = typeof(MatchingSignatures).GetMethod("InvertByRef", BindingFlags.Static | BindingFlags.Public);
            MethodInfo two = typeof(MatchingSignatures).GetMethod("GetMeaning", BindingFlags.Static | BindingFlags.Public);

            // This is the actual pointer for the MethodDesc
            IntPtr methodDesc = one.MethodHandle.Value;

            int data = 42;
            void* foo = &data;
            foo = InvertByRef(foo);
            foo = GetMeaning(foo);

            // This one will be different the second time through
            foo = More.InvertByRef(&data);

            // While this is the same as the first two
            foo = More.GetMeaning(foo);
        }
    }
}
