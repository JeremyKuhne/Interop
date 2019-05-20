// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace Interop.Support.Trace
{
    public class DiagnosticsClient
    {
        private uint _processId;
        private string _pipeName;
        private uint _bufferSize;

        private const uint DefaultBufferSize = 256;

        /// <summary>
        /// Create a 
        /// </summary>
        /// <param name="processId">The process to trace. Default is the current process.</param>
        /// <param name="bufferSize">The circular buffer size used by in MB.</param>
        public DiagnosticsClient(int processId = -1, uint bufferSize = DefaultBufferSize)
        {
            if (processId == -1)
                processId = Process.GetCurrentProcess().Id;

            _processId = (uint)processId;
            _bufferSize = bufferSize;

            // This is the Windows name.
            _pipeName = $"dotnetcore-diagnostic-{processId}";
        }

        // DiagnosticsServerThread in coreclr/src/vm/diagnosticserver.cpp looks for requests and
        // passes them to EventPipeProtocolHelper for detail parsing.
        //
        //

        private Stream StartTrace(ReadOnlySpan<Provider> providers, out ulong sessionId)
        {
            Stream pipe = OpenPipe();
            ValueBinaryWriter writer = new ValueBinaryWriter(1024);

            // Format is
            // 

            writer.Write(new MessageHeader(DiagnosticMessageType.CollectEventPipeTracing, _processId));
            writer.Write(_bufferSize);

            // The output path would go here once that scenario is working.
            writer.Write(0);
            writer.Write(providers.Length);
            foreach (var provider in providers)
            {
                writer.Write(provider.Keywords);
                writer.Write(provider.EventLevel);
                writer.Write(provider.Name.Length + 1);
                writer.WriteString(provider.Name);
                if (provider.FilterData == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(provider.FilterData.Length + 1);
                    writer.WriteString(provider.FilterData);
                }
            }

            pipe.Write(writer.Buffer);
            pipe.Flush();
            writer.Dispose();
            if (!pipe.TryRead(out sessionId))
            {
                throw new InvalidOperationException("Failed to successfully start the session.");
            }
            return pipe;
        }

        public EventPipeEventSource StartTracing(out ulong sessionId, params Provider[] providers)
        {
            return new EventPipeEventSource(StartTrace(providers, out sessionId));
        }

        public ulong StopTracing(ulong sessionId)
        {
            using (Stream pipe = OpenPipe())
            {
                pipe.Write(new MessageHeader(DiagnosticMessageType.StopEventPipeTracing, _processId));
                pipe.Write(sessionId);
                pipe.Flush();
                pipe.TryRead(out ulong result);
                return result;
            }
        }

        private Stream OpenPipe()
        {
            // Windows only, Unix communicates via Socket
            var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipe.Connect(3000);
            return pipe;
        }
    }
}
