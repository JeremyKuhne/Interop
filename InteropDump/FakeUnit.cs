using System;

namespace Xunit
{
    public class FactAttribute : Attribute
    {
    }

    public static class Assert
    {
        public static void Equal<T>(T expected, T actual) { }
        public static void NotEqual<T>(T expected, T actual) { }
    }
}
