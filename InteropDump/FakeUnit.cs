// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Xunit
{
    // Simple stubs to allow including Xunit attributed code without including Xunit.

    public class FactAttribute : Attribute
    {
    }

    public static class Assert
    {
        public static void Equal<T>(T expected, T actual) { }
        public static void NotEqual<T>(T expected, T actual) { }
    }
}
