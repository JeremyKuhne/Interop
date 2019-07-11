// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class ErrorCases
    {
        [DllImport(Libraries.Kernel32)]
        internal static extern void ThisDoesNotExist();

        [Fact]
        public void NotInvokingUndefinedMethodOk()
        {
            // Putting in a case that the compiler / JIT isn't likely to remove
            // but that we know is always false.

            if (DateTime.Now < new DateTime())
            {
                ThisDoesNotExist();
            }
        }

        [Fact]
        public void InvokingUndefinedMethodThrows()
        {
            Assert.Throws<EntryPointNotFoundException>(() => ThisDoesNotExist());
        }

        [DllImport("Mxyzptlk")]
        internal static extern void ThisAlsoDoesNotExist();

        [Fact]
        public void NotInvokingUndefinedLibraryOk()
        {
            // Putting in a case that the compiler / JIT isn't likely to remove
            // but that we know is always false.

            if (DateTime.Now < new DateTime())
            {
                ThisAlsoDoesNotExist();
            }
        }

        [Fact]
        public void InvokingUndefinedLibraryThrows()
        {
            Assert.Throws<DllNotFoundException>(() => ThisAlsoDoesNotExist());
        }
    }
}
