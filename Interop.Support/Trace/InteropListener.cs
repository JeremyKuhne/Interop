// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;
using System.Reflection;

namespace Interop.Support.Trace
{
    public class InteropListener
    {
        public static EventSource RuntimeEventSource =
            Type.GetType("System.Diagnostics.Tracing.NativeRuntimeEventSource")
            .GetField("Log", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as EventSource;

        private SimpleListener _listener;

        public InteropListener()
        {
            _listener = new SimpleListener();
            _listener.EventWritten += EventListener_EventWritten;
        }

        public event EventHandler<ILStubGeneratedEventArgs> EventWritten;

        public void EnableEvents()
        {
            _listener.EnableEvents(RuntimeEventSource, EventLevel.Verbose, (EventKeywords)0x2000);
        }

        public void DisableEvents()
        {
            _listener.DisableEvents(RuntimeEventSource);
        }

        private void EventListener_EventWritten(object sender, EventWrittenEventArgs e)
        {
            EventWritten?.Invoke(sender, new ILStubGeneratedEventArgs(e));
        }

        private class SimpleListener : EventListener
        {
        }
    }
}
