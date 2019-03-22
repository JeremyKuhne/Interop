// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace PerfTest
{
    public class StructOrEnum
    {
        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern int RawInvert(int value);

        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern bool MarshalInvert(bool value);

        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern ENUMBOOL EnumInvert(ENUMBOOL value);

        internal enum ENUMBOOL : int
        {
            FALSE = 0,
            TRUE = 1,
        }

        [DllImport(Libraries.NativeLibrary, EntryPoint = "Invert")]
        private static extern STRUCTBOOL StructInvert(STRUCTBOOL value);

        // This is slower than using an enum and even slower than letting the interop
        // layer convert 'bool' so why use it?
        // 
        // The biggest reasons are convenience and correctness. It just works with
        // 'bool' and you'll never trip up and compare BOOL != TRUE (only '0' is
        // FALSE). In most scenarios the overhead for any method here is trivial.
        // What really matters is that larger structs that contain BOOL are blittable.
        // The overhead for non-blittable structures is much, much greater than what
        // we're looking at here.

        public readonly struct STRUCTBOOL
        {
            private readonly int _value;
            public STRUCTBOOL(bool value) => _value = value ? 1 : 0;
            public static implicit operator bool(STRUCTBOOL value) => value._value != 0;
            public static implicit operator STRUCTBOOL(bool value) => new STRUCTBOOL(value);
            public override string ToString() => ((bool)this).ToString();
        }

        [Params(true, false)]
        public bool Value;

        [Benchmark(Baseline = true)]
        public bool InvertMarshal() => MarshalInvert(Value);

        [Benchmark]
        public bool InvertEnum() => EnumInvert(Value ? ENUMBOOL.TRUE : ENUMBOOL.FALSE) != ENUMBOOL.FALSE;

        [Benchmark]
        public bool InvertStruct() => StructInvert(Value);

        [Benchmark]
        public bool InvertRaw() => RawInvert(Value ? 1 : 0) != 0;

        // |        Method | Value |     Mean |     Error |    StdDev | Ratio | RatioSD |
        // |-------------- |------ |---------:|----------:|----------:|------:|--------:|
        // | InvertMarshal | False | 5.220 ns | 0.1343 ns | 0.1256 ns |  1.00 |    0.00 |
        // |    InvertEnum | False | 4.569 ns | 0.1418 ns | 0.1393 ns |  0.87 |    0.03 |
        // |  InvertStruct | False | 6.006 ns | 0.1225 ns | 0.1146 ns |  1.15 |    0.04 |
        // |     InvertRaw | False | 4.434 ns | 0.1139 ns | 0.1066 ns |  0.85 |    0.02 |
        // |               |       |          |           |           |       |         |
        // | InvertMarshal |  True | 5.104 ns | 0.1372 ns | 0.1283 ns |  1.00 |    0.00 |
        // |    InvertEnum |  True | 4.467 ns | 0.1396 ns | 0.1306 ns |  0.88 |    0.04 |
        // |  InvertStruct |  True | 6.039 ns | 0.0711 ns | 0.0665 ns |  1.18 |    0.04 |
        // |     InvertRaw |  True | 4.450 ns | 0.1412 ns | 0.1387 ns |  0.87 |    0.04 |
    }
}
