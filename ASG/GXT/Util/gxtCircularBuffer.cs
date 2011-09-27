using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT
{
    /// <summary>
    /// A simple circular buffer data structure for processing streams or 
    /// keeping collections at a fixed size
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // have the ability to wrap around and overwrite data or it defeats the purpose
    public class gxtCircularBuffer<T> : IEnumerable<T>
    {
        private T[] buffer;
        private int capacity;
        private int writeIndex;
        private int readIndex;

        public gxtCircularBuffer(int size = 128)
        {
            gxtDebug.Assert(size > 0);
            capacity = size + 1;
            buffer = new T[capacity];
            writeIndex = 0;
            readIndex = 0;
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            Array.Clear(buffer, 0, capacity);
        }

        public bool IsEmpty { get { return readIndex == writeIndex; } }

        public bool IsFull { get { return ((writeIndex + 1) % capacity) == readIndex; } }

        public int Capacity { get { return capacity; } }

        public bool Enqueue(T data)
        {
            bool isFull = IsFull;
            if (!isFull)
            {
                buffer[writeIndex] = data;
                writeIndex++;
                writeIndex %= capacity;
            }
            return isFull;
        }

        public T Peek()
        {
            return buffer[readIndex];
        }

        public T Dequeue()
        {
            T data = buffer[readIndex];
            readIndex++;
            readIndex %= capacity;
            return data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new gxtCircularBufferEnumerator(this);
        }


        public class gxtCircularBufferEnumerator : IEnumerator<T>
        {
            private gxtCircularBuffer<T> circularBuffer;
            private int curIndex;

            public gxtCircularBufferEnumerator(gxtCircularBuffer<T> buffer)
            {
                circularBuffer = buffer;
                curIndex = 0;
            }

            public T Current
            {
                get { return circularBuffer.buffer[curIndex]; }
            }

            public void Dispose()
            {

            }

            object System.Collections.IEnumerator.Current
            {
                get { return circularBuffer.buffer[curIndex]; }
            }

            public bool MoveNext()
            {
                if (curIndex < circularBuffer.capacity)
                {
                    curIndex++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                curIndex = 0;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
