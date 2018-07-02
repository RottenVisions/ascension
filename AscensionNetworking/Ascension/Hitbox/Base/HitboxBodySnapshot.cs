using UnityEngine;

namespace Ascension.Networking
{
    partial struct HitboxBodySnapshot
    {
        public const int MAX_HITBOXES = 16;
        public const int MAX_BODIES = 128;
        public HitboxBody Body;
        public Matrix4x4 Hitbox0_LTW;
        public Matrix4x4 Hitbox0_WTL;
        public Matrix4x4 Hitbox1_LTW;
        public Matrix4x4 Hitbox1_WTL;
        public Matrix4x4 Hitbox10_LTW;
        public Matrix4x4 Hitbox10_WTL;
        public Matrix4x4 Hitbox11_LTW;
        public Matrix4x4 Hitbox11_WTL;
        public Matrix4x4 Hitbox12_LTW;
        public Matrix4x4 Hitbox12_WTL;
        public Matrix4x4 Hitbox13_LTW;
        public Matrix4x4 Hitbox13_WTL;
        public Matrix4x4 Hitbox14_LTW;
        public Matrix4x4 Hitbox14_WTL;
        public Matrix4x4 Hitbox15_LTW;
        public Matrix4x4 Hitbox15_WTL;
        public Matrix4x4 Hitbox2_LTW;
        public Matrix4x4 Hitbox2_WTL;
        public Matrix4x4 Hitbox3_LTW;
        public Matrix4x4 Hitbox3_WTL;
        public Matrix4x4 Hitbox4_LTW;
        public Matrix4x4 Hitbox4_WTL;
        public Matrix4x4 Hitbox5_LTW;
        public Matrix4x4 Hitbox5_WTL;
        public Matrix4x4 Hitbox6_LTW;
        public Matrix4x4 Hitbox6_WTL;
        public Matrix4x4 Hitbox7_LTW;
        public Matrix4x4 Hitbox7_WTL;
        public Matrix4x4 Hitbox8_LTW;
        public Matrix4x4 Hitbox8_WTL;
        public Matrix4x4 Hitbox9_LTW;
        public Matrix4x4 Hitbox9_WTL;

        public void Raycast(RaycastHitsCollection hits)
        {
            if (0 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 0, ref Hitbox0_WTL, ref Hitbox0_LTW);

            if (1 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 1, ref Hitbox1_WTL, ref Hitbox1_LTW);

            if (2 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 2, ref Hitbox2_WTL, ref Hitbox2_LTW);

            if (3 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 3, ref Hitbox3_WTL, ref Hitbox3_LTW);

            if (4 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 4, ref Hitbox4_WTL, ref Hitbox4_LTW);

            if (5 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 5, ref Hitbox5_WTL, ref Hitbox5_LTW);

            if (6 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 6, ref Hitbox6_WTL, ref Hitbox6_LTW);

            if (7 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 7, ref Hitbox7_WTL, ref Hitbox7_LTW);

            if (8 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 8, ref Hitbox8_WTL, ref Hitbox8_LTW);

            if (9 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 9, ref Hitbox9_WTL, ref Hitbox9_LTW);

            if (10 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 10, ref Hitbox10_WTL, ref Hitbox10_LTW);

            if (11 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 11, ref Hitbox11_WTL, ref Hitbox11_LTW);

            if (12 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 12, ref Hitbox12_WTL, ref Hitbox12_LTW);

            if (13 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 13, ref Hitbox13_WTL, ref Hitbox13_LTW);

            if (14 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 14, ref Hitbox14_WTL, ref Hitbox14_LTW);

            if (15 >= Body.hitboxCount)
            {
                return;
            }
            Raycast(hits, Body, 15, ref Hitbox15_WTL, ref Hitbox15_LTW);
        }
    }
}