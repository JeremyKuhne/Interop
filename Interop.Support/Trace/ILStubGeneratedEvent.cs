// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;

namespace Interop.Support.Trace
{
    public class ILStubGeneratedEventArgs : EventArgs
    {
        // This event is generated in ILStubState::FinishEmit()
        //
        // PCODE MethodDesc::DoPrestub(MethodTable *pDispatchingMT)
        // PCODE GetStubForInteropMethod(MethodDesc* pMD, DWORD dwStubFlags, MethodDesc **ppStubMD)
        // PCODE NDirect::GetStubForILStub(NDirectMethodDesc* pNMD, MethodDesc** ppStubMD, DWORD dwStubFlags)
        // MethodDesc* NDirect::CreateCLRToNativeILStub()
        // MethodDesc* CreateInteropILStub()
        // static void CreateNDirectStubWorker()
        // void FinishEmit(MethodDesc* pStubMD)
        // |-EtwOnILStubGenerated
        //   |-FireEtwILStubGenerated
        //     |-EventPipeWriteEventILStubGenerated

        private EventWrittenEventArgs _event;

        public ILStubGeneratedEventArgs(EventWrittenEventArgs eventArgs)
        {
            _event = eventArgs;
        }

        public ushort ClrInstanceId => (ushort)_event.Payload[0];
        public ulong ModuleId => (ulong)_event.Payload[1];
        public ulong StubMethodID => (ulong)_event.Payload[2];
        public ILStubGeneratedFlagsMap StubFlags => (ILStubGeneratedFlagsMap)_event.Payload[3];
        public uint ManagedInteropMethodToken => (uint)_event.Payload[4];
        public string ManagedInteropMethodNamespace => (string)_event.Payload[5];
        public string ManagedInteropMethodName => (string)_event.Payload[6];
        public string ManagedInteropMethodSignature => (string)_event.Payload[7];
        public string NativeMethodSignature => (string)_event.Payload[8];
        public string StubMethodSignature => (string)_event.Payload[9];
        public string StubMethodILCode => (string)_event.Payload[10];
    }
}
