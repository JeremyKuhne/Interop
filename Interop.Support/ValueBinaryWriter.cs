// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Interop.Support
{
    public ref struct ValueBinaryWriter
    {
        private byte[] _arrayToReturnToPool;
        private Span<byte> _bytes;
        private int _pos;

        public ValueBinaryWriter(Span<byte> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _bytes = initialBuffer;
            _pos = 0;
        }

        public ValueBinaryWriter(int initialCapacity)
        {
            _arrayToReturnToPool = null;
            _bytes = default;
            _pos = 0;
            EnsureCapacity(initialCapacity);
        }

        public int Position => _pos;

        public int Capacity => _bytes.Length;

        public ReadOnlySpan<byte> Buffer => _bytes;

        public void EnsureCapacity(int capacity)
        {
            if (capacity > _bytes.Length)
                Grow(capacity - _pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _bytes.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            byte[] poolArray = ArrayPool<byte>.Shared.Rent(Math.Max(_pos + additionalCapacityBeyondPos, _bytes.Length * 2));

            _bytes.CopyTo(poolArray);

            byte[] toReturn = _arrayToReturnToPool;
            _bytes = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<byte>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            byte[] toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                ArrayPool<byte>.Shared.Return(toReturn);
            }
        }

        public unsafe void Write<T>(T value) where T : unmanaged
        {
            EnsureCapacity(_pos + sizeof(T));
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(_bytes.Slice(_pos)), value);
            _pos += sizeof(T);
        }

        public unsafe void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            // Need an extra two bytes for a null char
            int lengthInBytes = (value.Length + 1) * sizeof(char);
            EnsureCapacity(Position + lengthInBytes);
            fixed(char* c = value)
            {
                Span<byte> stringData = new Span<byte>(c, lengthInBytes);
                stringData.CopyTo(_bytes.Slice(_pos));
            }
            _pos += lengthInBytes;
        }
    }
}
