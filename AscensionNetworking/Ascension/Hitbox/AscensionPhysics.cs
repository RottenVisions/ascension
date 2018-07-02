using Ascension.Networking;
using UnityEngine;

namespace Ascension.Networking.Physics
{
    public static class AscensionPhysics
    {
        private static readonly int maxWorldSnapshots = 60;
        private static readonly ListExtended<AscensionHitboxBody> HitboxBodies = new ListExtended<AscensionHitboxBody>();

        private static readonly ListExtended<AscensionHitboxWorldSnapshot> WorldSnapshots =
            new ListExtended<AscensionHitboxWorldSnapshot>();

        internal static void RegisterBody(AscensionHitboxBody body)
        {
            HitboxBodies.AddLast(body);
        }

        internal static void UnregisterBody(AscensionHitboxBody body)
        {
            HitboxBodies.Remove(body);
        }

        internal static void SnapshotWorld()
        {
            Iterator<AscensionHitboxBody> it = HitboxBodies.GetIterator();
            AscensionHitboxWorldSnapshot sn = AscensionHitboxWorldSnapshot.Pool.Acquire();

            // set frame
            sn.frame = Core.Frame;

            // create snapshot
            while (it.Next())
            {
                sn.Snapshot(it.val);
            }

            WorldSnapshots.AddLast(sn);

            while (WorldSnapshots.Count > maxWorldSnapshots)
            {
                WorldSnapshots.RemoveFirst().Dispose();
            }
        }

        internal static void DrawSnapshot()
        {
#if DEBUG
            if (WorldSnapshots.Count > 0)
            {
                WorldSnapshots.First.Draw();
            }
#endif
        }

        internal static AscensionPhysicsHits Raycast(Ray ray)
        {
            if (WorldSnapshots.Count > 0)
            {
                return Raycast(ray, WorldSnapshots.Last);
            }

            return AscensionPhysicsHits.Pool.Acquire();
        }

        internal static AscensionPhysicsHits Raycast(Ray ray, int frame)
        {
            Iterator<AscensionHitboxWorldSnapshot> it = WorldSnapshots.GetIterator();

            while (it.Next())
            {
                if (it.val.frame == frame)
                {
                    return Raycast(ray, it.val);
                }
            }

            if (WorldSnapshots.Count > 0)
            {
                return Raycast(ray, WorldSnapshots.Last);
            }

            return AscensionPhysicsHits.Pool.Acquire();
        }

        internal static AscensionPhysicsHits OverlapSphere(Vector3 origin, float radius)
        {
            if (WorldSnapshots.Count > 0)
            {
                return OverlapSphere(origin, radius, WorldSnapshots.Last);
            }

            return AscensionPhysicsHits.Pool.Acquire();
        }

        internal static AscensionPhysicsHits OverlapSphere(Vector3 origin, float radius, int frame)
        {
            Iterator<AscensionHitboxWorldSnapshot> it = WorldSnapshots.GetIterator();

            while (it.Next())
            {
                if (it.val.frame == frame)
                {
                    return OverlapSphere(origin, radius, it.val);
                }
            }

            return AscensionPhysicsHits.Pool.Acquire();
        }

        private static AscensionPhysicsHits Raycast(Ray ray, AscensionHitboxWorldSnapshot sn)
        {
            Iterator<AscensionHitboxBodySnapshot> it = sn.bodySnapshots.GetIterator();
            AscensionPhysicsHits hits = AscensionPhysicsHits.Pool.Acquire();

            while (it.Next())
            {
                it.val.Raycast(ray.origin, ray.direction, hits);
            }

            return hits;
        }

        private static AscensionPhysicsHits OverlapSphere(Vector3 origin, float radius, AscensionHitboxWorldSnapshot sn)
        {
            Iterator<AscensionHitboxBodySnapshot> it = sn.bodySnapshots.GetIterator();
            AscensionPhysicsHits hits = AscensionPhysicsHits.Pool.Acquire();

            while (it.Next())
            {
                it.val.OverlapSphere(origin, radius, hits);
            }

            return hits;
        }
    }
}