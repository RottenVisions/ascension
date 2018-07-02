using System;
using Ascension.Networking;
using UnityEngine;

namespace Ascension.Networking.Physics
{
    public class AscensionHitboxBodySnapshot : NetObject, IDisposable
    {
        private static readonly NetObjectPool<AscensionHitboxBodySnapshot> Pool =
            new NetObjectPool<AscensionHitboxBodySnapshot>();

        private AscensionHitboxBody body;
        private int count;
        private Matrix4x4 ltw = Matrix4x4.identity;
        private Matrix4x4 wtl = Matrix4x4.identity;
        private readonly Matrix4x4[] hbltw = new Matrix4x4[32];
        private readonly Matrix4x4[] hbwtl = new Matrix4x4[32];

        public void Dispose()
        {
            body = null;
            count = 0;
            wtl = Matrix4x4.identity;
            ltw = Matrix4x4.identity;

            Array.Clear(hbwtl, 0, hbwtl.Length);
            Array.Clear(hbltw, 0, hbltw.Length);

            Pool.Release(this);
        }

        public void Snapshot(AscensionHitboxBody body)
        {
            this.body = body;
            count = Mathf.Min(body.hitboxes.Length, hbwtl.Length);

            if (body.proximity)
            {
                wtl = body.proximity.transform.worldToLocalMatrix;
                ltw = body.proximity.transform.localToWorldMatrix;
            }

            for (int i = 0; i < count; ++i)
            {
                hbwtl[i] = body.hitboxes[i].transform.worldToLocalMatrix;
                hbltw[i] = body.hitboxes[i].transform.localToWorldMatrix;
            }
        }

        public void OverlapSphere(Vector3 center, float radius, AscensionPhysicsHits hits)
        {
            if (!body)
            {
                return;
            }

            if (body.proximity)
            {
                if (body.proximity.OverlapSphere(ref wtl, center, radius))
                {
                    hits.AddHit(body, body.proximity, (center - ltw.MultiplyPoint(Vector3.zero)).magnitude);
                }
                else
                {
                    return;
                }
            }

            for (int i = 0; i < body.hitboxes.Length; ++i)
            {
                AscensionHitbox hitbox = body.hitboxes[i];

                if (hitbox.OverlapSphere(ref hbwtl[i], center, radius))
                {
                    hits.AddHit(body, hitbox, (center - hbltw[i].MultiplyPoint(Vector3.zero)).magnitude);
                }
            }
        }

        public void Raycast(Vector3 origin, Vector3 direction, AscensionPhysicsHits hits)
        {
            if (!body)
            {
                return;
            }

            float distance = float.NegativeInfinity;

            if (body.proximity)
            {
                if (body.proximity.Raycast(ref wtl, origin, direction, out distance))
                {
                    hits.AddHit(body, body.proximity, distance);
                }
                else
                {
                    return;
                }
            }

            for (int i = 0; i < body.hitboxes.Length; ++i)
            {
                AscensionHitbox hitbox = body.hitboxes[i];

                if (hitbox.Raycast(ref hbwtl[i], origin, direction, out distance))
                {
                    hits.AddHit(body, hitbox, distance);
                }
            }
        }

        public void Draw()
        {
#if DEBUG
            if (!body)
            {
                return;
            }

            if (body.proximity)
            {
                body.proximity.Draw(ltw);
            }

            for (int i = 0; i < count; ++i)
            {
                body.hitboxes[i].Draw(hbltw[i]);
            }
#endif
        }

        public static AscensionHitboxBodySnapshot Create(AscensionHitboxBody body)
        {
            AscensionHitboxBodySnapshot sn = Pool.Acquire();
            sn.Snapshot(body);
            return sn;
        }
    }
}