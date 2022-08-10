// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics.Tracing;

namespace Interop.Support.Trace;

/// <summary>
///  Diagnostics event provider data.
/// </summary>
/// <remarks>
///  The string format for a provider is "(GUID|KnownProviderName)[:Flags[:Level][:KeyValueArgs]]".
/// </remarks>
public readonly struct Provider
{
    public const string DotNetRuntime = "Microsoft-Windows-DotNETRuntime";

    /// <summary>
    ///  Tracing keyword flags for "Microsoft-Windows-DotNETRuntime".
    /// </summary>
    /// <remarks>
    ///  Described in ClrEtwAll.man <see cref="https://github.com/dotnet/runtime/blob/main/src/coreclr/vm/ClrEtwAll.man"/>.
    /// </remarks>
    public ClrTraceEventParser.Keywords Keywords { get; }

    public EventLevel EventLevel { get; }

    public string Name { get; }

    public string? FilterData { get; }

    public Provider(
        ClrTraceEventParser.Keywords keywords,
        string name = DotNetRuntime,
        EventLevel eventLevel = EventLevel.Verbose,
        string? filterData = null)
    {
        Keywords = keywords;
        Name = name;
        EventLevel = eventLevel;
        FilterData = filterData;
    }
}
