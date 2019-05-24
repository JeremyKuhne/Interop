// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Interop.Support.Trace
{
    /// <summary>
    /// Diagnostic message type.
    /// </summary>
    /// <remarks>
    /// This matches up with the struct in diagnosticserver.h.
    /// <see cref="https://github.com/dotnet/coreclr/blob/master/src/vm/diagnosticserver.h"/>
    /// </remarks>
    public enum DiagnosticMessageType : uint
    {
        StartEventPipeTracing = 1024,
        StopEventPipeTracing,
        CollectEventPipeTracing
    }
}
