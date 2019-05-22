// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class Strings
    {
        [DllImport(Libraries.NativeLibrary, CharSet = CharSet.Unicode)]
        private static extern IntPtr StringPass(string value, int length);

        [Fact]
        public unsafe void StringPins()
        {
            // ILWSTRMarshaler
            string foo = "foo";
            fixed (void* s = foo)
            {
                // native int (string, int32)
                Assert.Equal((IntPtr)s, StringPass(foo, foo.Length));
            }
        }

        // ---------------------------------------------------------------------------------
        // Interop IL for: private static extern IntPtr StringPass(string value, int length)
        // ---------------------------------------------------------------------------------
        //
        // The native method signature: unmanaged stdcall int64(native int, int64)
        //
        //      .maxstack 4
        //      .locals (
        //          int32,                                      // 0: Tracker for cleanup
        //          native int,                                 // 1: Address of first char in string
        //          string pinned,                              // 2: Pinned string pointer
        //          int64,                                      // 3: Outgoing length
        //          int64,                                      // 4: Return value
        //          int64)                                      // 5: Return value
        //
        //  *** Marshal the arguments ***
        //  -----------------------------
        //
        //   >> Put zero into locals[0] <<
        //
        //          ldc.i4.0                                    // push '0' int onto the stack
        //          stloc.0                                     // pop the zero into locals[0]
        //
        //   >> Handle the first argument (string) <<
        //
        // IL_0002: nop
        //
        //          >> Put IntPtr.Zero into locals[1] <<
        //
        //          ldc.i4.0                                    // push '0' int onto the stack
        //          conv.i                                      // convert the stack int into a native int (IntPtr)
        //          stloc.1                                     // pop the native int to locals[1]
        //
        //          >> If the string is null, goto handling the second argument <<
        //
        //          ldarg.0                                     // load argument 0 (string) onto the stack
        //          brfalse         IL_0015                     // pop if false (null) jump to IL_0015
        //
        //          >> Pin the string into locals[2] and put the address in locals[1] <<
        //
        //          ldarg.0                                     // load argument 0 (string) onto the stack
        //          stloc.2                                     // pop the string into locals[2] (pinned string)
        //          ldloc.2                                     // load the pinned string from locals[2]
        //          ldflda          System.String::_firstChar   // push the address of _firstChar to the stack
        //          stloc.1                                     // pop the address of the _firstChar to locals[1]
        //
        //   >> Handle the second argument (string) <<
        //
        // IL_0015: nop
        //          nop
        //
        //          >> Convert the length to a long and put it in locals[3] <<
        //
        //          ldarg.1                                     // push argument 1 (length) to stack
        //          conv.i8                                     // convert it to long
        //          stloc.3                                     // pop it to locals[3]
        //          nop
        //          nop
        //          nop
        //
        //  *** Invoke the native method ***
        //  --------------------------------
        //
        //   >> Push the arguments onto the stack <<
        //
        //          ldloc.1                                     // push locals[1] to stack (&_firstChar, the first arg)
        //          ldloc.3                                     // push locals[3] to stack (length, the second arg)
        //
        //   >> JIT intrinsic to fill in the actual method pointer (IL for identical P/Invokes is shared) <<
        //
        //          call            native int [System.Private.CoreLib] System.StubHelpers.StubHelpers::GetStubContext()  // JIT Intrinsic
        //          ldc.i4.s        0x20                        // push '0x20' int onto the stack
        //          add                                         // add '0x20' and ??? (some ** ?)
        //          ldind.i                                     // load (indirect) native int from stack address (e.g. *) to stack
        //          ldind.i                                     // again (should be the function pointer)
        //
        //   >> Call the native method
        //
        //         calli           unmanaged stdcall int64(native int,int64)
        //
        //  *** Unmarshal return value and arguments ***
        //  --------------------------------------------
        //
        //         nop
        //         stloc.s         0x5                         // pop the return value into locals[5]
        //         ldloc.s         0x5                         // push locals[5]
        //         stloc.s         0x4                         // pop to locals[4]
        //         ldloc.s         0x4                         // push locals[4]
        //         nop
        //         nop
        //         nop
        //         nop
        //         nop
        //         ret

        //
        //   First four integers are passed in RCX, RDX, R8, R9
        // https://github.com/dotnet/coreclr/blob/a1757ce8e80cd089d9dc31ba2d4e3246e387a6b8/Documentation/botr/clr-abi.md
        //
        // The VM Shares IL stubs based on signatures
        // 
        //   // IL stub's secret MethodDesc parameter (JitFlags::JIT_FLAG_PUBLISH_SECRET_PARAM)
        //   #define REG_SECRET_STUB_PARAM    REG_R10

        [DllImport(Libraries.NativeLibrary, EntryPoint = "StringPass", CharSet = CharSet.Unicode)]
        private static extern IntPtr StringPassIn([In]string value, int length);

        [Fact]
        public unsafe void StringInPins()
        {
            // ILWSTRMarshaler
            string foo = "foo";
            fixed (void* s = foo)
            {
                Assert.Equal((IntPtr)s, StringPassIn(foo, foo.Length));
            }
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "StringPass", CharSet = CharSet.Unicode)]
        private static unsafe extern void* StringPassNative(char* value, int length);

        [Fact]
        public unsafe void StringPassDirect()
        {
            // ILWSTRMarshaler
            string foo = "foo";
            fixed (char* s = foo)
            {
                // native int (string, int32)
                Assert.Equal((IntPtr)s, (IntPtr)StringPassNative(s, foo.Length));
            }
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "StringPass", CharSet = CharSet.Ansi)]
        private static extern IntPtr StringPassA(string value, int length);

        [Fact]
        public unsafe void AnsiDoesNotPin()
        {
            // This should be fairly obvious. To see this in action, set a Function breakpoint
            // on "ConvertToNative" after debugging into this method. You'll find that you stop
            // at System.StubHelpers.CSTRMarshaller.
            //
            // ILCSTRMarshaller is the marshalling generator for string conversion that inserts
            // the call to the managed CSTRMarshaller helper (in ilmarshallers.cpp).
            //
            // ILCSTRMarshaler::EmitConvertContentsCLRToNative(ILCodeStream* pslILEmit) 


            string foo = "foo";
            fixed (void* s = foo)
            {
                Assert.NotEqual((IntPtr)s, StringPassA(foo, foo.Length));
            }
        }
    }
}
