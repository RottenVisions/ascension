using System;
using System.Runtime.InteropServices;

namespace Ascension.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkValue
    {
        [FieldOffset(0)]
        public System.UInt64 Packed0;

        [FieldOffset(8)]
        public System.UInt64 Packed1;

        [FieldOffset(0)]
        public System.Boolean Bool;

        [FieldOffset(0)]
        public System.Int32 Int0;

        [FieldOffset(0)]
        public System.Single Float0;

        [FieldOffset(4)]
        public System.Single Float1;

        [FieldOffset(8)]
        public System.Single Float2;

        [FieldOffset(12)]
        public System.Single Float3;

        [FieldOffset(0)]
        public System.Guid Guid;

        [FieldOffset(0)]
        public NetworkId NetworkId;

        [FieldOffset(0)]
        public PrefabId PrefabId;

        [FieldOffset(0)]
        public NetworkTrigger TriggerLocal;

        [FieldOffset(8)]
        public NetworkTrigger TriggerSend;

        [FieldOffset(0)]
        public UnityEngine.Vector2 Vector2;

        [FieldOffset(0)]
        public UnityEngine.Vector3 Vector3;

        [FieldOffset(0)]
        public UnityEngine.Quaternion Quaternion;

        [FieldOffset(0)]
        public UnityEngine.Color Color;

        [FieldOffset(0)]
        public UnityEngine.Color32 Color32;

        [FieldOffset(12)]
        public Boolean HasCopiedFromTransform;

        [FieldOffset(16)]
        public System.Object Object;

        public bool HasNonDefaultValue
        {
            get { return (Packed0 != 0) || (Packed1 != 0) || (Object != null); }
        }

        public UnityEngine.Matrix4x4 Matrix4x4
        {
            get { return (UnityEngine.Matrix4x4)Object; }
            set { Object = value; }
        }

        public System.String String
        {
            get { return (System.String)Object; }
            set { Object = value; }
        }

        public IMessageRider ProtocolToken
        {
            get { return (IMessageRider)Object; }
            set { Object = value; }
        }

        public NetworkTransform Transform
        {
            get { return (NetworkTransform)Object; }
            set { Object = value; }
        }

        public System.Action Action
        {
            get { return (System.Action)Object; }
            set { Object = value; }
        }

        public AscensionEntity Entity
        {
            get
            {
                return AscensionNetwork.FindEntity(NetworkId);
            }
            set
            {
                if (value && value.IsAttached)
                {
                    NetworkId = value.NetworkId;
                }
                else
                {
                    NetworkId = new NetworkId();
                }
            }
        }

        public static bool Diff(Boolean a, Boolean b) { return a != b; }
        public static bool Diff(Int32 a, Int32 b) { return a != b; }
        public static bool Diff(Guid a, Guid b) { return a != b; }
        public static bool Diff(Single a, Single b) { return a != b; }
        public static bool Diff(NetworkId a, NetworkId b) { return a != b; }
        public static bool Diff(PrefabId a, PrefabId b) { return a != b; }
        public static bool Diff(NetworkTrigger a, NetworkTrigger b) { return a != b; }
        public static bool Diff(System.Object a, System.Object b) { return ReferenceEquals(a, b) == false; }

        public static bool Diff(UnityEngine.Vector2 a, UnityEngine.Vector2 b) { return a != b; }
        public static bool Diff(UnityEngine.Vector3 a, UnityEngine.Vector3 b) { return a != b; }
        public static bool Diff(UnityEngine.Quaternion a, UnityEngine.Quaternion b) { return a != b; }
        public static bool Diff(UnityEngine.Color a, UnityEngine.Color b) { return a != b; }
        public static bool Diff(UnityEngine.Color32 a, UnityEngine.Color32 b) { return a.r != b.r || a.b != b.b || a.g != b.g || a.a != b.a; }
        public static bool Diff(UnityEngine.Matrix4x4 a, UnityEngine.Matrix4x4 b) { return a != b; }

        public static bool Diff_Strict(UnityEngine.Vector2 a, UnityEngine.Vector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static bool Diff_Strict(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        public static bool Diff_Strict(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
        }
    }
}
