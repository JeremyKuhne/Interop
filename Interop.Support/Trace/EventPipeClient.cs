// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO.Pipes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Interop.Support.Trace;

/// <summary>
///  Client for the .NET event pipe.
/// </summary>
/// <remarks>
///  This is a programmatic way to control the event pipe. The event pipe can be controlled from the
///  command line by setting environment variables. The relevant variables are:
///
///   - DOTNET_EnableEventPipe=1   : Enable EventPipe immediately for the life of the process.
///   - DOTNET_EventPipeConfig     : Provides the configuration in xperf string form for which providers/keywords/levels to be enabled.
///                                     If not specified, the default configuration is used.
///   - DOTNET_EventPipeConfig_EventPipeOutputFile : The full path to the netperf file to be written.
///   - DOTNET_EventPipeConfig_EventPipeCircularMB : The size in megabytes of the circular buffer.
///  
///  Provider format: "(GUID|KnownProviderName)[:Flags[:Level][:KeyValueArgs]]"
///                   e.g. "Microsoft-Windows-DotNETRuntime:0x2000:5"
/// </remarks>
public class EventPipeClient
{
    private uint _processId;
    private string _pipeName;
    private uint _bufferSize;

    private const uint DefaultBufferSize = 256;

    /// <summary>
    /// Create a new client instance
    /// </summary>
    /// <param name="processId">The process to trace. Default is the current process.</param>
    /// <param name="bufferSize">The circular buffer size used by in MB.</param>
    public EventPipeClient(int processId = -1, uint bufferSize = DefaultBufferSize)
    {
        if (processId == -1)
            processId = Process.GetCurrentProcess().Id;

        _processId = (uint)processId;
        _bufferSize = bufferSize;

        // This is the Windows name.
        _pipeName = $"dotnetcore-diagnostic-{processId}";
    }

    /// <summary>
    ///  Start a trace session for the given <paramref name="providers"/>.
    /// </summary>
    /// <remarks>
    ///  The format is the same as in the ".netperf" files and is described here:
    ///  https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/EventPipe/EventPipeFormat.md
    /// 
    ///  This stream can be fed into <see cref="EventPipeEventSource"/>
    ///  which can then be parsed with <see cref="ClrTraceEventParser"/>
    /// 
    ///  DiagnosticsServerThread in coreclr/src/vm/diagnosticserver.cpp looks for requests and
    ///  passes them to EventPipeProtocolHelper for detail parsing.
    /// </remarks>
    private Stream StartSession(ReadOnlySpan<Provider> providers, out ulong sessionId)
    {
        Stream pipe = OpenPipe();
        ValueBinaryWriter writer = new(1024);

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
            if (provider.FilterData is null)
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

    public Stream StartSession(out ulong sessionId, params Provider[] providers)
    {
        return StartSession(providers, out sessionId);
    }

    public ulong StopSession(ulong sessionId)
    {
        using Stream pipe = OpenPipe();
        pipe.Write(new MessageHeader(DiagnosticMessageType.StopEventPipeTracing, _processId));
        pipe.Write(sessionId);
        pipe.Flush();
        pipe.TryRead(out ulong result);
        return result;
    }

    private Stream OpenPipe()
    {
        // Windows only, Unix communicates via Socket
        var pipe = new NamedPipeClientStream(_pipeName);

        // Impersonation requires the following invokation:
        // new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
        pipe.Connect(3000);
        return pipe;
    }
}
