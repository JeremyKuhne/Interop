// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace InteropTest;

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

    [Fact]
    public void DefaultImportAttributes()
    {
        // This is what the IL looks like for "private static extern int Double(int value);"
        //
        //   .method private hidebysig static pinvokeimpl("NativeLibrary.dll" winapi)
        //           int32  Double(int32 'value') cil managed preservesig
        //   {
        //   }
        //
        // [DllImport] is a "Pseudo" Attribute (ECMA-335 I I.21.2.1 - see PseudoCustomAttribute
        // in System.Private.CoreLib). It sets the "pinvokeimpl" bit for the method, which has
        // the following IL definition:
        //
        //   pinvokeimpl ‘(’ QSTRING [ as QSTRING ] PinvAttr * ‘)’
        //
        // Valid PinvAttr options are:
        //
        //  ansi | autochar | unicode                           (i.e. CharSet)
        //  [pmCharSetAnsi | pmCharSetAuto | pmCharSetUnicode]
        //  cdecl | fastcall | stdcall | thiscall | platformapi (i.e. CallingConvention)
        //  nomangle                                            (i.e. ExactSpelling = true)
        //  lasterr                                             (i.e. SetLastError = true)
        //
        // "pinvokeimpl" metadata is stored in the "ImplMap" metadata table, which maps to the
        // runtime flags defined in "CorPinvokeMap"
        //
        //   il               | value  | CorPinvokeMap (corhdr.h)
        //   -----------------|--------|----------------------------------
        //   nomangle         | 0x0001 | pmNoMangle
        //   ansi             | 0x0002 | pmCharSetAnsi
        //   unicode          | 0x0004 | pmCharSetUnicode
        //   autochar         | 0x0006 | pmCharSetAuto
        //   lasterr          | 0x0040 | pmSupportsLastError
        //   winapi           | 0x0100 | pmCallConvWinapi
        //   cdecl            | 0x0200 | pmCallConvCdecl
        //   stdcall          | 0x0300 | pmCallConvStdcall
        //   thiscall         | 0x0400 | pmCallConvThiscall
        //   fastcall         | 0x0500 | pmCallConvFastcall
        //    [** below are not part of ECMA-335 and only apply to ANSI on Windows **]
        //   bestfit:on       | 0x0010 | pmBestFitEnabled
        //   bestfit:off      | 0x0020 | pmBestFitDisabled
        //   charmaperror:on  | 0x1000 | pmThrowOnUnmappableCharEnabled
        //   charmaperror:off | 0x2000 | pmThrowOnUnmappableCharDisabled
        //
        // PInvokeStaticSigInfo::DllImportInit reads in the metadata for the PInvoke. If CharSet is unspecified,
        // this method assumes ANSI. If CharSet is Auto, it becomes Unicode for Windows and ANSI for Unix.
        // 
        // ANSI conversion ultimately goes through Marshal.StringToAnsiString() and 
        // Unix gets UTF-8 conversion as the ANSI conversion code in the runtime's Unix PAL (unicode.cpp) presumes
        // the active code page is UTF-8.

        MethodInfo mi = typeof(Basics).GetMethod("Double", BindingFlags.NonPublic | BindingFlags.Static)!;
        DllImportAttribute import = mi.GetCustomAttribute<DllImportAttribute>()!;
        Assert.False(import!.BestFitMapping);
        Assert.Equal(CallingConvention.Winapi, import.CallingConvention);
        Assert.Equal(CharSet.None, import.CharSet);
        Assert.Equal("Double", import.EntryPoint);
        Assert.False(import.ExactSpelling);
        Assert.True(import.PreserveSig);
        Assert.False(import.SetLastError);
        Assert.Equal("NativeLibrary.dll", import.Value);

        // These are not described in the ECMA-335 specification
        Assert.False(import.ThrowOnUnmappableChar);
        Assert.False(import.BestFitMapping);


        // CSTRMarshaler


        // ThrowOnUnmappableChar and BestFitMapping ultimately map to WideCharToMultiByte in Marshal.StringToAnsiString().
        //
        //   https://docs.microsoft.com/en-us/windows/desktop/api/stringapiset/nf-stringapiset-widechartomultibyte
        //
        // Note that on Unix the CoreCLR implements this in /pal/src/locale/unicode.cpp. BestFitMapping is not supported.
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
