// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics.Tracing;
using System.Reflection;

namespace Interop.Support.Trace;

/// <summary>
///  Simple listener for listening to interop trace events.
/// </summary>
public class InteropListener
{
    public static EventSource RuntimeEventSource { get; } =
        (EventSource)Type.GetType("System.Diagnostics.Tracing.NativeRuntimeEventSource")!
        .GetField("Log", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    private readonly SimpleListener _listener;

    public InteropListener()
    {
        _listener = new SimpleListener();
        _listener.EventWritten += EventListener_EventWritten;
    }

    /// <summary>
    ///  Fired when an interop event is written.
    /// </summary>
    public event EventHandler<ILStubGeneratedEventArgs>? EventWritten;

    /// <summary>
    ///  Start listening to interop trace events.
    /// </summary>
    public void EnableEvents()
    {
        _listener.EnableEvents(RuntimeEventSource, EventLevel.Verbose, (EventKeywords)ClrTraceEventParser.Keywords.Interop);
    }

    /// <summary>
    ///  Stop listening to interop trace events.
    /// </summary>
    public void DisableEvents()
    {
        _listener.DisableEvents(RuntimeEventSource);
    }

    private void EventListener_EventWritten(object? sender, EventWrittenEventArgs e)
    {
        if (e.EventName?.Equals("ILStubGenerated") ?? false)
        {
            EventWritten?.Invoke(sender, new ILStubGeneratedEventArgs(e));
        }
    }

    private class SimpleListener : EventListener
    {
    }
}
