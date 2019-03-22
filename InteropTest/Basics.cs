// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class Basics
    {
        [DllImport(Libraries.NativeLibrary)]
        private static extern int Double(int value);

        [Fact]
        public void SimpleImport()
        {
            int doubled = Double(6);
            Assert.Equal(12, doubled);
        }

        // When looking for an import, the expected method name is
        // a literal match. You can specify the name via "EntryPoint".

        [DllImport(Libraries.NativeLibrary, EntryPoint = "Double")]
        private static extern int TimesTwo(int value);

        [Fact]
        public void ExplicitEntryPoint()
        {
            int doubled = TimesTwo(6);
            Assert.Equal(12, doubled);
        }

        // By *default* char/string is treated as ANSI, *not* Unicode. Always
        // specify CharSet with imports or types that contain char to ensure
        // types are marshalled correctly and efficiently.
        //
        // The other interesting thing that we're demonstrating here is that
        // the interop layer will automatically search for the name with
        // either an "A" or a "W" appended (in this case the real name is
        // "CharToIntW"). This is done as Windows APIs that take strings
        // usually have two separate entry points, one for Unicode (W) and
        // one for ANSI (A). The "root" name is actually a define to either
        // the "A" or the "W" API based on whether or not "UNICODE" is defined.
        //
        // You can avoid this probing by setting "ExactSpelling" to "true".

        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode /*, ExactSpelling = true */)]
        private static extern int CharToInt(char value);

        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int CharToIntW(char value);

        [Fact]
        public void CharInterop()
        {
            int value = CharToInt('A');
            Assert.Equal(65, value);
        }

        [DllImport(Libraries.NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DoubleCDeclImplicit(int value);

        [Fact]
        public void CdeclImport()
        {
            // Windows uses stdcall in the *vast* majority of it's APIs.
            int doubled = DoubleCDeclImplicit(5);
            Assert.Equal(10, doubled);
        }

        // Calling convention is StdCall by default
        [DllImport(Libraries.NativeLibrary)]
        private static extern int AddCDecl(int a, int b);

        [Fact]
        public void WrongConvention()
        {
            // Uh-oh, but only on 32 bit. Cdecl/Stdcall/etc. have no meaning on 64 bit.
            // Windows has one way of making calls in 64 bit, Unix has another.
            //
            // On 32 bit there is a managed debugging assistant that tries to catch stack
            // imbalance problems due to mismatched arguments or calling conventions.
            // https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/pinvokestackimbalance-mda

            int sum = AddCDecl(9, 7);
            Assert.Equal(16, sum);
        }
    }
}
