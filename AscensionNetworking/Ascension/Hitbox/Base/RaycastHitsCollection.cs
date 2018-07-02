using System;
using UnityEngine;

namespace Ascension.Networking
{
    public struct ARaycastHit
    {
        public HitboxBody body;
        public float distance;
        public Hitbox hitbox;
    }

    public class RaycastHitsCollection : IDisposable
    {
        internal ARaycastHit[] hitsArray = new ARaycastHit[HitboxBodySnapshot.MAX_BODIES / 2];
        internal int hitsCount;
        internal Ray ray;

        public int Count
        {
            get { return hitsCount; }
        }

        public ARaycastHit this[int index]
        {
            get
            {
                if (index >= hitsCount)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return hitsArray[index];
            }
        }

        public void Dispose()
        {
        }

        internal void Add(HitboxBody body, Hitbox hitbox, float distance)
        {
            if (hitsCount + 1 == hitsArray.Length)
            {
                Array.Resize(ref hitsArray, hitsArray.Length * 2);
            }

            hitsArray[hitsCount].body = body;
            hitsArray[hitsCount].hitbox = hitbox;
            hitsArray[hitsCount].distance = distance;
        }
    }
}