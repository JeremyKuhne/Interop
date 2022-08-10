// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Interop.Support;

public static class StreamExtensions
{
    public static unsafe bool TryRead<T>(this Stream stream, out T value) where T : unmanaged
    {
        T buffer = default;
        value = default;

        Span<byte> tempSpan = new(&buffer, sizeof(T));
        if (stream.Read(tempSpan) != sizeof(T))
        {
            return false;
        }

        value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(tempSpan));
        return true;
    }

    public static unsafe void Write<T>(this Stream stream, T value) where T : unmanaged
    {
        stream.Write(new ReadOnlySpan<byte>(&value, sizeof(T)));
    }

    public static unsafe void WriteString(this Stream stream, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        int lengthInBytes = (value.Length + 1) * sizeof(char);
        fixed (char* c = value)
        {
            Span<byte> stringData = new(c, lengthInBytes);
            stream.Write(stringData);
        }
    }
}
