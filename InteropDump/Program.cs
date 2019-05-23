// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Interop.Support.Trace;
using InteropTest;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace InteropDump
{
    /// <summary>
    /// Test projects don't respect the launchSettings.json so we can't set environment variables
    /// for the test run. This project sets the logging variables so we can explore the dumped
    /// trace information.
    /// </summary>
    /// <remarks>
    /// launchSettings.json needs the following environment variables to log to disk:
    ///
    ///     "environmentVariables": {
    ///         "COMPlus_EventPipeConfig": "Microsoft-Windows-DotNETRuntime:0x2000:5",
    ///         "COMPlus_EnableEventPipe": "1"
    ///     },
    ///
    /// These can be set via the debug tab in the project properties.
    /// </remarks>
    class Program
    {
        static void Main(string[] args)
        {
            int id = System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine($"Process id {id}.");

            InteropListener listener = new InteropListener();
            listener.EventWritten += Listener_EventWritten;
            listener.EnableEvents();

            Tests.Test();

            // Sleep a bit to give the event a chance to post.
            Thread.Sleep(1000);
        }

        private static void Listener_EventWritten(object sender, ILStubGeneratedEventArgs e)
        {
            // This won't come back on the same thread that we created the listener.
            Console.WriteLine($"Stub generated for {e.ManagedInteropMethodNamespace}.{e.ManagedInteropMethodName}");
        }

        public static void EtwListener(ManualResetEvent started, int processId)
        {
            // This goes through the normal Windows ETW tracing. It works pretty well, but is a bit heavy and
            // any session will outlive the process if you don't explicitly shut it down. This will lead to
            // a lack of resources to start a new listener. You can iterate through and close them if needed:
            //
            //    var names = TraceEventSession.GetActiveSessionNames();
            //    foreach (string name in names)
            //    {
            //        if (name.StartsWith("MySession_"))
            //        {
            //            TraceEventSession.GetActiveSession(name).Stop();
            //        }
            //    }
            //
            // This uses the Microsoft.Diagnostics.Tracing.TraceEvent NuGet package.

            using (TraceEventSession session = new TraceEventSession($"MySession_{Guid.NewGuid().ToString()}"))
            {
                TraceEventProviderOptions options = new TraceEventProviderOptions { ProcessIDFilter = new List<int> { processId } };
                session.Source.Clr.ILStubStubGenerated += (ILStubGeneratedTraceData data) =>
                {
                    started.Set();
                    if (data.ManagedInteropMethodNamespace.StartsWith("InteropTest"))
                    {
                        Console.WriteLine("ILStub");
                    }
                };

                session.EnableProvider(ClrTraceEventParser.ProviderGuid, matchAnyKeywords: (ulong)ClrTraceEventParser.Keywords.Interop);

                // This call blocks.
                session.Source.Process();
            }
        }

        public static void PipeListener(ManualResetEvent started, int processId)
        {
            // You can also register for traditional events through IPC. This is new to .NET Core 3.0.
            // "dotnet-trace" is the command-line tool for listening. There currently isn't a public surface
            // area, but I have implemented one (DiagnosticsClient) in Interop.Support.

            EventPipeClient client = new EventPipeClient(processId);

            Stream stream = client.StartSession(
                out ulong sessionId,
                new Provider(ClrTraceEventParser.Keywords.Interop));
            started.Set();

            // This will hang up waiting for the first object to arrive so we have to set the event before this.
            using (var eventSource = new EventPipeEventSource(stream))
            {
                ClrTraceEventParser parser = new ClrTraceEventParser(eventSource);
                parser.ILStubStubGenerated += (ILStubGeneratedTraceData data) =>
                {
                    Console.WriteLine("ILStub");
                };

                // This call blocks.
                eventSource.Process();
            }
        }
    }

    public static class Tests
    {
        // The method is here to avoid getting inlined before we hook events.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Test()
        {
            Strings s = new Strings();
            s.StringPins();
            s.StringPins();
        }
    }
}
