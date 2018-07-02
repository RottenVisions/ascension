using UnityEngine;

namespace Ascension.Networking
{
    partial struct HitboxBodySnapshot
    {
        public void Raycast(RaycastHitsCollection hits, HitboxBody body, int box, ref Matrix4x4 wtl, ref Matrix4x4 ltw)
        {
            Hitbox hitbox = body.hitboxArray[box];
            if (hitbox.shape == HitboxShape.Sphere)
            {
            }
            else
            {
                Bounds b = new Bounds(hitbox.center, hitbox.size);
                float d = 0f;

                if (b.IntersectRay(hits.ray, out d))
                {
                    hits.Add(body, hitbox, d);
                }
            }
        }
    }
}