// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Interop.Support.Trace
{
    /// <summary>
    /// Header for messages sent to the CLR diagnostics server.
    /// </summary>
    /// <remarks>
    /// This matches up with the struct in diagnosticserver.h.
    /// <see cref="https://github.com/dotnet/coreclr/blob/master/src/vm/diagnosticserver.h"/>
    /// </remarks>
    public readonly struct MessageHeader
    {
        public readonly DiagnosticMessageType MessageType;
        public readonly uint ProcessId;

        public MessageHeader(DiagnosticMessageType messageType, uint processId)
        {
            MessageType = messageType;
            ProcessId = processId;
        }
    }
}
