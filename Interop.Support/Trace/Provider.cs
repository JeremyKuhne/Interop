// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics.Tracing;

namespace Interop.Support.Trace
{
    // EventPipeProviderConfiguration (EventPipeProtocolHelper::TryParseProviderConfiguration)
    //
    //  uint countConfigs
    //      ulong keyords
    //      uint logLevel
    //      w_char* providerName
    //      w_char* filterData (optional)

    /// The following environment variables are used to configure EventPipe:
    ///  - COMPlus_EnableEventPipe=1 : Enable EventPipe immediately for the life of the process.
    ///  - COMPlus_EventPipeConfig : Provides the configuration in xperf string form for which providers/keywords/levels to be enabled.
    ///                              If not specified, the default configuration is used.
    ///  - COMPlus_EventPipeOutputFile : The full path to the netperf file to be written.
    ///  - COMPlus_EventPipeCircularMB : The size in megabytes of the circular buffer.
    ///  Microsoft-Windows-DotNETRuntime

    // Provider format: "(GUID|KnownProviderName)[:Flags[:Level][:KeyValueArgs]]"
    // where KeyValueArgs are of the form: "[key1=value1][;key2=value2]"
    // `strConfig` must be of the form "Provider[,Provider]"

    // .netperf
    // https://github.com/Microsoft/perfview/blob/master/src/TraceEvent/EventPipe/EventPipeFormat.md

    public readonly struct Provider
    {
        public const string DotNetRuntime = "Microsoft-Windows-DotNETRuntime";

        /// <summary>
        /// Tracing keyword flags for "Microsoft-Windows-DotNETRuntime".
        /// </summary>
        /// <remarks>
        /// Described in ClrEtwAll.man <see cref="https://github.com/dotnet/coreclr/blob/master/src/vm/ClrEtwAll.man"/>.
        /// </remarks>
        public ClrTraceEventParser.Keywords Keywords { get; }

        public EventLevel EventLevel { get; }

        public string Name { get; }

        public string FilterData { get; }

        public Provider(ClrTraceEventParser.Keywords keywords, string name = DotNetRuntime, EventLevel eventLevel = EventLevel.Verbose, string filterData = null)
        {
            Keywords = keywords;
            Name = name;
            EventLevel = eventLevel;
            FilterData = filterData;
        }
    }
}
