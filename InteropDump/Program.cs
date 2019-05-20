// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Interop.Support.Trace;
using InteropTest;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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
            var names = TraceEventSession.GetActiveSessionNames();
            foreach (string name in names)
            {
                if (name.StartsWith("MySession_"))
                {
                    TraceEventSession.GetActiveSession(name).Stop();
                }
            }
            //var session = TraceEventSession.GetActiveSession("NetCore");
            //session = new TraceEventSession($"MySession_{Guid.NewGuid().ToString()}", TraceEventSession)

            int id = System.Diagnostics.Process.GetCurrentProcess().Id;

            var task = Task.Run(() =>
            {
                using (TraceEventSession session = new TraceEventSession($"MySession_{Guid.NewGuid().ToString()}"))
                {
                    // TraceEventProviderOptions options = new TraceEventProviderOptions { ProcessIDFilter = }
                    session.Source.Clr.ILStubStubGenerated += (ILStubGeneratedTraceData data) =>
                    {
                        Console.WriteLine("ILStub");
                    };

                    session.EnableProvider(ClrTraceEventParser.ProviderGuid, matchAnyKeywords: (ulong)ClrTraceEventParser.Keywords.Interop);
                    session.Source.Process();
                }
                //    DiagnosticsClient client = new DiagnosticsClient();
                //using (var eventSource = client.StartTracing(out ulong sessionId, new Provider(ClrTraceEventParser.Keywords.All, name: "*")))
                //{
                //    ClrTraceEventParser parser = new ClrTraceEventParser(eventSource);
                //    parser.ILStubStubGenerated += (ILStubGeneratedTraceData data) =>
                //    {
                //    };

                //    parser.All += (TraceEvent @event) =>
                //    {
                //    };

                //    eventSource.Process();
                //    //parser.
                //    //int read = 0;
                //    //byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
                //    //do
                //    //{
                //    //    read = reader.Read(buffer);
                //    //    if (read > 0)
                //    //    {
                //    //        string data = Convert.ToBase64String(buffer, 0, read);
                //    //    }
                //    //    Thread.Yield();
                //    //} while (read >= 0);
                //    //ArrayPool<byte>.Shared.Return(buffer);
                //}
            });

            while (task.Status != TaskStatus.Running)
            {
                Thread.Yield();
            }

            Tests.Test();
            while (true)
            {
                Thread.Yield();
            };
        }
    }



    public static class Tests
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Test()
        {
            Strings s = new Strings();
            s.StringPins();
        }
    }
}
