// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Running;

namespace PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<KeepAlive>();
        }
    }
}
