using System;

namespace Ascension.Networking
{
    /// <summary>
    /// Contains the indices for all arrays that contain a specific property, in order
    /// </summary>
    public struct ArrayIndices
    {
        readonly int[] indices;

        public ArrayIndices(int[] indices)
        {
            this.indices = indices;
        }

        /// <summary>
        /// The amount of indices
        /// </summary>
        public int Length
        {
            get { return (indices == null) ? 0 : indices.Length; }
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return this.indices[index];
            }
        }
    }
}