using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking
{
    public struct DoubleBuffer<T> where T : struct
    {
        public T Previous;
        public T Current;

        public DoubleBuffer<T> Shift(T value)
        {
            DoubleBuffer<T> buffer = this;
            buffer.Previous = Current;
            buffer.Current = value;
            return buffer;
        }

        public static DoubleBuffer<T> InitBuffer(T value)
        {
            DoubleBuffer<T> buffer;
            buffer.Previous = value;
            buffer.Current = value;
            return buffer;
        }
    }
}
