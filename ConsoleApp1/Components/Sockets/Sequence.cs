using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public class Sequence<T> where T : unmanaged
    {
        private T[] _array= [];
        private int _offset;
        private int _count;

        public Sequence() : this([]) { }

        public Sequence(T[] array)
        {
            Reset(array);
        }

        public int Offset => _offset;
        public int Count => _count;
        public int CurrentLength => Math.Max(_count - _offset, 0);

        public bool TryRead(out T value)
        {
            if (_offset < _count)
            {
                value = _array[_offset++];
                return true;
            }

            value = default;
            return false;
        }

        public void Advance(int count) => _offset = Math.Min(_offset + count, _count);

        public void Rewind(int count) => _offset = Math.Max(_offset - count, 0);

        public void Rewind() => _offset = 0;

        public void Read(T[] newBuffer, int startIndex = 0)
        {
            int index = 0;
            while (index < newBuffer.Length && TryRead(out var value))
            {
                newBuffer[startIndex + index] = value;
                index++;
            }
        }

        public void Append(T[] array)
        {
            int newLength = _array.Length + array.Length;
            Array.Resize(ref _array, newLength);
            Buffer.BlockCopy(array, 0, _array, _array.Length - array.Length, array.Length * Unsafe.SizeOf<T>());
            _count = newLength;
        }

        internal void RemovePreviousOffset()
        {
            if (_offset == 0 || _count == 0 || CurrentLength == 0)
            {
                Reset([]);
                return;
            }

            var newArray = new T[CurrentLength];
            Buffer.BlockCopy(_array, _offset * Unsafe.SizeOf<T>(), newArray, 0, newArray.Length * Unsafe.SizeOf<T>());
            Reset(newArray);
        }

        public bool End() => _count == 0 || CurrentLength == 0;

        public Sequence<T> Slice(int start, int length = -1)
        {
            if (start >= _offset)
                return new Sequence<T>([]);

            length = length == -1 ? _count - start : length;
            var array = new T[length];
            Read(array, start);
            return new Sequence<T>(array);
        }

        public T[] AsOffsetArray()
        {
            var array = new T[CurrentLength];
            Buffer.BlockCopy(_array, _offset * Unsafe.SizeOf<T>(), array, 0, array.Length * Unsafe.SizeOf<T>());
            return array;
        }

        public T[] AsArray() => _array;

        private void Reset(T[] array)
        {
            _array = array;
            _count = array.Length;
            _offset = 0;
        }
    }
}
