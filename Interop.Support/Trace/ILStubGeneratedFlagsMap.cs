// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Interop.Support.Trace;

public enum ILStubGeneratedFlagsMap : uint
{
    ReverseInterop      = 0x01,
    COMInterop          = 0x02,
    NGenedStub          = 0x04,
    DelegateMap         = 0x08,
    VarArg              = 0x10,
    UnmanagedCallee     = 0x20
}
