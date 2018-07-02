using System;
using UnityEngine;

namespace Ascension.Networking
{
    public enum HitboxShape
    {
        Box,
        Sphere
    }

    [Serializable]
    public class Hitbox
    {
        public Transform bone = null;
        public Vector3 center = Vector3.zero;
        public float radius = 0.25f;
        public HitboxShape shape = HitboxShape.Box;
        public Vector3 size = new Vector3(0.25f, 0.25f, 0.25f);
    }

    public class HitboxBody : MonoBehaviour
    {
        [SerializeField] internal Hitbox[] hitboxArray = new Hitbox[HitboxBodySnapshot.MAX_HITBOXES];
        [SerializeField] internal int hitboxCount;
    }
}