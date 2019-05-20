using System;
using System.Collections.Generic;
using System.Text;

namespace Interop.Support.Trace
{
    // Defined in coreclr/src/vm/fastserializer.h
    public enum EventTag : byte
    {
        Error = 0,
        NullReference = 1,
        ObjectReference = 2,
        ForwardReference = 3,
        BeginObject = 4,
        BeginPrivateObject = 5,
        EndObject = 6,
        ForwardDefinition = 7,
        Byte = 8,
        Int16 = 9,
        Int32 = 10,
        Int64 = 11,
        SkipRegion = 12,
        String = 13,
        Blob = 14,
        Limit
    }
}
