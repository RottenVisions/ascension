using System;
using Ascension.Networking;
using Ascension.Networking.Physics;

namespace Ascension.Networking.Physics
{
    public class AscensionHitboxWorldSnapshot : NetObject, IDisposable
    {
        internal static readonly NetObjectPool<AscensionHitboxWorldSnapshot> Pool =
            new NetObjectPool<AscensionHitboxWorldSnapshot>();

        internal int frame;

        internal ListExtendedSingular<AscensionHitboxBodySnapshot> bodySnapshots =
            new ListExtendedSingular<AscensionHitboxBodySnapshot>();

        public void Dispose()
        {
            while (bodySnapshots.Count > 0)
            {
                bodySnapshots.RemoveFirst().Dispose();
            }

            frame = 0;
            Pool.Release(this);
        }

        internal void Snapshot(AscensionHitboxBody body)
        {
            bodySnapshots.AddLast(AscensionHitboxBodySnapshot.Create(body));
        }

        public void Draw()
        {
#if DEBUG
            Iterator<AscensionHitboxBodySnapshot> it = bodySnapshots.GetIterator();

            while (it.Next())
            {
                it.val.Draw();
            }
#endif
        }
    }
}