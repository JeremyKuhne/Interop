// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest;

/// <summary>
///  Passing `BOOL` as `bool` has overhead. Passing as an int or an
///  enum is measurably faster, but can have subtle bugs as you cannot
///  compare to `TRUE`. Using a struct is possible and safer, but
///  much slower than leaving to the marshaller.
/// </summary>
public class StructOrEnum
{
    [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
    private static extern int RawInvert(int value);

    [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
    private static extern bool MarshalInvert(bool value);

    [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
    private static extern ENUMBOOL EnumInvert(ENUMBOOL value);

    [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
    private static extern STRUCTBOOL StructInvert(STRUCTBOOL value);

    [Params(true, false)]
    public bool Value;

    [Benchmark(Baseline = true)]
    public bool InvertMarshal() => MarshalInvert(Value);

    [Benchmark]
    public bool InvertEnum() => EnumInvert(Value.ToEnumBool()).IsTrue();

    [Benchmark]
    public bool InvertEnumNoHelpers() => EnumInvert(Value ? ENUMBOOL.TRUE : ENUMBOOL.FALSE) != ENUMBOOL.FALSE;

    [Benchmark]
    public bool InvertStruct() => StructInvert(Value);

    [Benchmark]
    public bool InvertRaw() => RawInvert(Value ? 1 : 0) != 0;

    // |              Method | Value |     Mean |     Error |    StdDev | Ratio | RatioSD |
    // |-------------------- |------ |---------:|----------:|----------:|------:|--------:|
    // |       InvertMarshal | False | 5.712 ns | 0.0509 ns | 0.0425 ns |  1.00 |    0.00 |
    // |          InvertEnum | False | 5.240 ns | 0.0274 ns | 0.0257 ns |  0.92 |    0.01 |
    // | InvertEnumNoHelpers | False | 5.320 ns | 0.1158 ns | 0.1026 ns |  0.93 |    0.02 |
    // |        InvertStruct | False | 6.523 ns | 0.0357 ns | 0.0334 ns |  1.14 |    0.01 |
    // |           InvertRaw | False | 5.252 ns | 0.0808 ns | 0.0717 ns |  0.92 |    0.02 |
    // |                     |       |          |           |           |       |         |
    // |       InvertMarshal |  True | 5.695 ns | 0.0319 ns | 0.0299 ns |  1.00 |    0.00 |
    // |          InvertEnum |  True | 5.483 ns | 0.1218 ns | 0.1140 ns |  0.96 |    0.02 |
    // | InvertEnumNoHelpers |  True | 5.220 ns | 0.0323 ns | 0.0302 ns |  0.92 |    0.00 |
    // |        InvertStruct |  True | 6.418 ns | 0.0426 ns | 0.0378 ns |  1.13 |    0.01 |
    // |           InvertRaw |  True | 5.313 ns | 0.1330 ns | 0.1244 ns |  0.93 |    0.02 |
}

// Using a struct is slower than using an enum and even slower than letting
// the interop layerr convert 'bool'. The marshaller treats a struct of a
// single field equivalently to that field when returning/passing, which
// isn't technically correct in all cases, but works.
//
// Why use it? Convenience. Currently it isn't possible to make operator
// extension methods for implicit conversion to bool (with an enum). Encapsulation
// makes it harder to accidentally compare BOOL != TRUE (only '0' is FALSE).

public readonly struct STRUCTBOOL
{
    private readonly int _value;
    public STRUCTBOOL(bool value) => _value = value ? 1 : 0;
    public static implicit operator bool(STRUCTBOOL value) => value._value != 0;
    public static implicit operator STRUCTBOOL(bool value) => new STRUCTBOOL(value);
    public override string ToString() => ((bool)this).ToString();
}

public enum ENUMBOOL : int
{
    FALSE = 0,
    TRUE = 1,
}

// Extension classes can't be nested
public static class EnumBoolExtensions
{
    public static bool IsTrue(this ENUMBOOL value) => value != ENUMBOOL.FALSE;
    public static bool IsFalse(this ENUMBOOL value) => value == ENUMBOOL.FALSE;

    public static ENUMBOOL ToEnumBool(this bool value) => value ? ENUMBOOL.TRUE : ENUMBOOL.FALSE;
}
