// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace InteropTest;

/// <summary>
///  Exercise in translating ELEMENTDESC and VARIANT to C#.
/// </summary>
public class ElemDesc
{
    // typedef struct tagELEMDESC {
    //     TYPEDESC tdesc;             /* the type of the element */
    //     union {
    //         IDLDESC idldesc;        /* info for remoting the element */
    //         PARAMDESC paramdesc;    /* info about the parameter */
    //     } DUMMYUNIONNAME;
    // } ELEMDESC, * LPELEMDESC;

    public struct ELEMDESC
    {
        public TYPEDESC tdesc;
        public UnionType union;

        [StructLayout(LayoutKind.Explicit)]
        public struct UnionType
        {
            [FieldOffset(0)]
            public IDLDESC idldesc;
            [FieldOffset(0)]
            public PARAMDESC paramdesc;
        }
    }

    [Fact]
    public unsafe void ElemDescTest()
    {
        // TYPEDESC is the size of two pointers (see the TypeDescTest below).
        // ILDESC and PARAMDESC are both the size of two pointers due to
        // packing rules. Given their union we have a total size of 4 pointers.
        Assert.Equal(IntPtr.Size * 4, sizeof(ELEMDESC));
    }

    // typedef struct tagTYPEDESC
    // {
    // /* [switch_is][switch_type] */ union 
    //     {
    //     /* [case()] */ struct tagTYPEDESC *lptdesc;
    //     /* [case()] */ struct tagARRAYDESC *lpadesc;
    //     /* [case()] */ HREFTYPE hreftype;
    //     /* [default] */  /* Empty union arm */ 
    //     } DUMMYUNIONNAME;
    // VARTYPE vt;
    // } TYPEDESC;

    public struct TYPEDESC
    {
        public UnionType Union;

        // Extra data to describe some VARTYPEs The size of this union
        // is the same as a pointer (IntPtr.Size).
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct UnionType
        {
            // For VT_SAFEARRAY and VT_PTR
            [FieldOffset(0)]
            public TYPEDESC* lptdesc;

            // For describing a VT_CARRAY
            [FieldOffset(0)]
            public ARRAYDESC* lpadesc;

            // This union member is for user defined types (VT_USERDEFINED)
            [FieldOffset(0)]
            public HREFTYPE hreftype;
        }

        public VARTYPE vt;
    }

    [Fact]
    public unsafe void TypeDescTest()
    {
        Assert.Equal(IntPtr.Size, sizeof(TYPEDESC.UnionType));
        Assert.Equal(2, sizeof(VARTYPE));

        // sizeof(TYPEDESC) gives 16 (IntPtr.Size * 2) for 64 bit C++ compilation
        // This is because packing requires an even multiple of the largest native
        // size, so even though VARTYPE is only 2 bytes (ushort) another 2 to 6
        // bytes are needed for packing.

        Assert.Equal(IntPtr.Size * 2, sizeof(TYPEDESC));

        // The vt field rests right after the union, so it is always a pointer size
        // in from the start of the struct.

        TYPEDESC typeDesc = new TYPEDESC();
        ulong offset = (ulong)(void*)&typeDesc.vt - (ulong)(void*)&typeDesc;
        Assert.Equal((ulong)IntPtr.Size, offset);
    }

    // typedef struct tagARRAYDESC
    // {
    //     TYPEDESC tdescElem;
    //     USHORT cDims;
    //     /* [size_is] */
    //     SAFEARRAYBOUND rgbounds[1];
    // } ARRAYDESC;

    // https://docs.microsoft.com/openspecs/windows_protocols/ms-oaut/2e06e2b6-054e-48b1-b867-ad1e87a7ebe2
    public struct ARRAYDESC
    {
        public TYPEDESC tdescElem;
        public ushort cDims;

        // This is actually an anysize array. It can be represented by a fixed size array if you know
        // exactly how many items there are in your given usage, but it is usually better to use pointers
        // with this setup.
        //
        // Because of this there is no "size" of this struct, even though the C++ definition gives it
        // an array size of 1. Taking sizeof() in C#/C++ will match as it is defined here.

        public SAFEARRAYBOUND rgbounds;

        public unsafe void SampleIterator(ARRAYDESC* desc)
        {
            // Using span to look at bounds of each of the dimensions as returned from native code
            // gives you additional bounds checking and allows using foreach.

            var bounds = new ReadOnlySpan<SAFEARRAYBOUND>(&desc->rgbounds, desc->cDims);
            for (int i = 0; i < bounds.Length; i++)
            {
                Console.WriteLine($"Dimension {i} has {bounds[i].cElements} elements.");
            }

            SAFEARRAYBOUND* boundsPointer = &desc->rgbounds;
            for (int i = 0; i < desc->cDims; i++)
            {
                Console.WriteLine($"Dimension {i} has {boundsPointer[i].cElements} elements.");
            }
        }
    }

    // typedef struct tagSAFEARRAYBOUND
    // {
    //     ULONG cElements;
    //     LONG lLbound;
    // } SAFEARRAYBOUND;

    public struct SAFEARRAYBOUND
    {
        // On Windows C++ compilers long is the same size as int
        public uint cElements;
        public int lLbound;
    }

    [Fact]
    public unsafe void SafeArrayBoundTest()
    {
        // I.e. two ints size
        Assert.Equal(8, sizeof(SAFEARRAYBOUND));
    }

    // typedef DWORD HREFTYPE;

    public enum HREFTYPE : uint
    {
    }

    // typedef unsigned short VARTYPE;

    public enum VARTYPE : ushort
    {
        VT_EMPTY = 0,
        VT_NULL = 1,
        VT_I2 = 2,
        VT_I4 = 3,
        VT_R4 = 4,
        VT_R8 = 5,
        VT_CY = 6,
        VT_DATE = 7,
        VT_BSTR = 8,
        VT_DISPATCH = 9,
        VT_ERROR = 10,
        VT_BOOL = 11,
        VT_VARIANT = 12,
        VT_UNKNOWN = 13,
        VT_DECIMAL = 14,  // 1110
        VT_I1 = 16,
        VT_UI1 = 17,
        VT_UI2 = 18,
        VT_UI4 = 19,
        VT_I8 = 20,
        VT_UI8 = 21,
        VT_INT = 22,
        VT_UINT = 23,
        VT_VOID = 24,
        VT_HRESULT = 25,
        VT_PTR = 26,
        VT_SAFEARRAY = 27,
        VT_CARRAY = 28,
        VT_USERDEFINED = 29,
        VT_LPSTR = 30,
        VT_LPWSTR = 31,
        VT_RECORD = 36,
        VT_INT_PTR = 37,
        VT_UINT_PTR = 38,
        VT_FILETIME = 64,
        VT_BLOB = 65,
        VT_STREAM = 66,
        VT_STORAGE = 67,
        VT_STREAMED_OBJECT = 68,
        VT_STORED_OBJECT = 69,
        VT_BLOB_OBJECT = 70,
        VT_CF = 71,
        VT_CLSID = 72,
        VT_VERSIONED_STREAM = 73,
    }

    // typedef struct tagIDLDESC
    // {
    //     ULONG_PTR dwReserved;
    //     USHORT wIDLFlags;
    // } IDLDESC;

    public struct IDLDESC
    {
        // Technically uint*, but it's reserved and we don't need to look at it.
        // Making it IntPtr allows this struct to not need unsafe.
        public IntPtr dwReserved;
        public ushort wIDLFlags;
    }

    // typedef struct tagPARAMDESC
    // {
    //     LPPARAMDESCEX pparamdescex;
    //     USHORT wParamFlags;
    // } PARAMDESC;

    public unsafe struct PARAMDESC
    {
        public PARAMDESC* pparamdescex;
        public ushort wParamFlags;
    }

    // typedef struct tagPARAMDESCEX
    // {
    //     ULONG cBytes;
    //     VARIANTARG varDefaultValue;
    // } PARAMDESCEX;

    public struct PARAMDESCEX
    {
        public uint cBytes;
        public VARIANT varDefaultValue;
    }

    // typedef VARIANT VARIANTARG;

    // struct tagVARIANT
    // {
    // union
    //     {
    //     struct __tagVARIANT
    //         {
    //         VARTYPE vt;
    //         WORD wReserved1;
    //         WORD wReserved2;
    //         WORD wReserved3;
    //         union 
    //             {
    //             LONGLONG llVal;
    //             LONG lVal;
    //             BYTE bVal;
    //             SHORT iVal;
    //             FLOAT fltVal;
    //             DOUBLE dblVal;
    //             VARIANT_BOOL boolVal;
    //             VARIANT_BOOL __OBSOLETE__VARIANT_BOOL;
    //             SCODE scode;
    //             CY cyVal;
    //             DATE date;
    //             BSTR bstrVal;
    //             IUnknown *punkVal;
    //             IDispatch *pdispVal;
    //             SAFEARRAY *parray;
    //             BYTE *pbVal;
    //             SHORT *piVal;
    //             LONG *plVal;
    //             LONGLONG *pllVal;
    //             FLOAT *pfltVal;
    //             DOUBLE *pdblVal;
    //             VARIANT_BOOL *pboolVal;
    //             VARIANT_BOOL *__OBSOLETE__VARIANT_PBOOL;
    //             SCODE *pscode;
    //             CY *pcyVal;
    //             DATE *pdate;
    //             BSTR *pbstrVal;
    //             IUnknown **ppunkVal;
    //             IDispatch **ppdispVal;
    //             SAFEARRAY **pparray;
    //             VARIANT *pvarVal;
    //             PVOID byref;
    //             CHAR cVal;
    //             USHORT uiVal;
    //             ULONG ulVal;
    //             ULONGLONG ullVal;
    //             INT intVal;
    //             UINT uintVal;
    //             DECIMAL *pdecVal;
    //             CHAR *pcVal;
    //             USHORT *puiVal;
    //             ULONG *pulVal;
    //             ULONGLONG *pullVal;
    //             INT *pintVal;
    //             UINT *puintVal;
    //             struct __tagBRECORD
    //                 {
    //                 PVOID pvRecord;
    //                 IRecordInfo *pRecInfo;
    //                 } __VARIANT_NAME_4;
    //             } __VARIANT_NAME_3;
    //         } __VARIANT_NAME_2;
    //     DECIMAL decVal;
    //     } __VARIANT_NAME_1;
    // } ;

    [StructLayout(LayoutKind.Explicit)]
    public struct VARIANT
    {
        [FieldOffset(0)]
        public VariantName1 Data;

        // "decimal" is the same size (16 bytes) and layout as DECIMAL
        // The first ushort of space is "reserved" and that is actually
        // used to allow overlapping with the VARTYPE in the main
        // part of the union. .NET has the exact same layout for
        // System.Decimal.

        [FieldOffset(0)]
        public decimal decVal;

        public struct VariantName1
        {
            // The first four values are 8 bytes total (4 ushorts).
            public VARTYPE vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;

            // The union that actually holds the data (other than decimal)
            // has as its largest value the BRECORD, which is two pointers.
            // This makes the union 8 bytes on 32 bit and 16 bytes on
            // 64 bit. Together with the 8 bytes above this gives the
            // total struct size of 16/24 bytes on 32/64 bit. (Decimal is
            // unioned with this, but isn't any larger than this fork
            // of the union.)
            public UnionType Value;

            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
            public unsafe struct UnionType
            {
                [FieldOffset(0)]
                public long llVal;
                [FieldOffset(0)]
                public int lVal;
                [FieldOffset(0)]
                public byte bVal;
                [FieldOffset(0)]
                public short iVal;
                [FieldOffset(0)]
                public float fltVal;
                [FieldOffset(0)]
                public double dblVal;
                // VARIANT_BOOL is a short
                [FieldOffset(0)]
                public short boolVal;
                // SCODE is an int
                [FieldOffset(0)]
                public int scode;
                // CY is a long
                [FieldOffset(0)]
                public long cyVal;
                // DATE is a double
                [FieldOffset(0)]
                public double date;
                // BSTR is a char* (null terminated)
                [FieldOffset(0)]
                public char* bstrVal;
                // punkVal is IUnknown*
                [FieldOffset(0)]
                public void* punkVal;
                // pdispVal is IDispatch
                [FieldOffset(0)]
                public void* pdispVal;
                [FieldOffset(0)]
                public SAFEARRAY* parray;
                [FieldOffset(0)]
                public byte* pbVal;
                [FieldOffset(0)]
                public short* piVal;
                [FieldOffset(0)]
                public int* plVal;
                [FieldOffset(0)]
                public long* pllVal;
                [FieldOffset(0)]
                public float* pfltVal;
                [FieldOffset(0)]
                public double* pdblVal;
                [FieldOffset(0)]
                public short* pboolVal;
                // SCODE is an int
                [FieldOffset(0)]
                public int* pscode;
                // CY is a long
                [FieldOffset(0)]
                public long* pcyVal;
                // DATE is a double
                [FieldOffset(0)]
                public double* pdate;
                // BSTR is a char* (null terminated)
                [FieldOffset(0)]
                public char** pbstrVal;
                // ppunkVal is IUnknown**
                [FieldOffset(0)]
                public void** ppunkVal;
                // ppdispVal is IDispatch**
                [FieldOffset(0)]
                public void** ppdispVal;
                [FieldOffset(0)]
                public SAFEARRAY** pparray;
                [FieldOffset(0)]
                public VARIANT* pvarVal;
                [FieldOffset(0)]
                public void* byref;
                [FieldOffset(0)]
                public sbyte cVal;
                [FieldOffset(0)]
                public ushort uiVal;
                [FieldOffset(0)]
                public uint ulVal;
                [FieldOffset(0)]
                public ulong ullVal;
                [FieldOffset(0)]
                public int intVal;
                [FieldOffset(0)]
                public uint uintVal;
                [FieldOffset(0)]
                public decimal* pdecVal;
                [FieldOffset(0)]
                public sbyte* pcVal;
                [FieldOffset(0)]
                public ushort* puiVal;
                [FieldOffset(0)]
                public uint* pulVal;
                [FieldOffset(0)]
                public ulong* pullVal;
                [FieldOffset(0)]
                public int* pintVal;
                [FieldOffset(0)]
                public uint* puintVal;
                [FieldOffset(0)]
                public BRECORD brecord;
                public unsafe struct BRECORD
                {
                    public void* pvRecord;
                    // pRecInfo is IRecordInfo*
                    public void* pRecInfo;
                }
            }
        }
    }

    [Fact]
    public unsafe void VariantTest()
    {
        if (Environment.Is64BitProcess)
        {
            Assert.Equal(24, sizeof(VARIANT));
        }
        else
        {
            Assert.Equal(16, sizeof(VARIANT));
        }
    }

    // typedef struct tagSAFEARRAY
    // {
    //     USHORT cDims;
    //     USHORT fFeatures;
    //     ULONG cbElements;
    //     ULONG cLocks;
    //     PVOID pvData;
    //     SAFEARRAYBOUND rgsabound[1];
    // } SAFEARRAY;

    public unsafe struct SAFEARRAY
    {
        public ushort cDims;
        public ushort fFeatures;
        public uint cbElements;
        public uint cLocks;
        public void* pvData;

        // This is an anysize array (see comments on ARRAYDISC for more on these)
        public SAFEARRAYBOUND rgsabound;
    }
}
