using System;
using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    public static class NetMath
    {
        /// That's are two versions: a quadratic bezier(one control point) and a cubic bezier(two control points). 
        /// Each comes in two versions: Vector2 and Vector3.
        /// The cubic equatation is transformed so you don't have those ugly (1-t)(1-t)(1-t). 
        /// When you just multiply the brackets you get (1 + (-3 +(3-t)*t)*t) which looks even more ugly but with 
        /// the other terms it resolves quite nicely ;)
        /// http://answers.unity3d.com/questions/12689/moving-an-object-along-a-bezier-curve.html
        public static Vector2 Bezier2(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            return (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
        }

        public static Vector3 Bezier2(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            return (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
        }

        public static Vector2 Bezier3(Vector2 start, Vector2 startControl, Vector2 endControl, Vector2 end, float t)
        {
            return (((-start + 3 * (startControl - endControl) + end) * t + (3 * (start + endControl) - 6 * startControl)) * t + 3 * (startControl - start)) * t + start;
        }

        public static Vector3 Bezier3(Vector3 start, Vector3 startControl, Vector3 endControl, Vector3 end, float t)
        {
            return (((-start + 3 * (startControl - endControl) + end) * t + (3 * (start + endControl) - 6 * startControl)) * t + 3 * (startControl - start)) * t + start;
        }

        #region Extended Ascension Math
        public static float InterpolateFloat(ListExtended<NetworkStorage> frames, int offset, int frame, bool angle)
        {
            var f0 = frames.First;
            var p0 = f0.Values[offset].Float1;

            if ((frames.Count == 1) || (f0.Frame >= frame))
            {
                return p0;
            }
            else
            {
                var f1 = frames.Next(f0);
                var p1 = f1.Values[offset].Float1;

                NetAssert.True(f1.Frame > f0.Frame);
                NetAssert.True(f1.Frame > frame);

                int f0Frame = f0.Frame;
                if (f0Frame < (f1.Frame - Core.RemoteSendRate * 2))
                {
                    f0Frame = f1.Frame - Core.RemoteSendRate * 2;
                }

                float t = f1.Frame - f0Frame;
                float d = frame - f0Frame;

                return angle ? Mathf.LerpAngle(p0, p1, d / t) : Mathf.Lerp(p0, p1, d / t);
            }
        }

        public static Vector3 InterpolateVector(ListExtended<NetworkStorage> frames, int offset, int frame, float snapLimit)
        {
            bool snapped = false;
            return InterpolateVector(frames, offset, frame, snapLimit, ref snapped);
        }

        public static Vector3 InterpolateVector(ListExtended<NetworkStorage> frames, int offset, int frame, float snapLimit, ref bool snapped)
        {
            var f0 = frames.First;
            var p0 = f0.Values[offset].Vector3;

            if ((frames.Count == 1) || (f0.Frame >= frame))
            {
                return p0;
            }
            else
            {
                var f1 = frames.Next(f0);
                var p1 = f1.Values[offset].Vector3;

                NetAssert.True(f1.Frame > f0.Frame);
                NetAssert.True(f1.Frame > frame);

                if ((p0 - p1).sqrMagnitude > (snapLimit * snapLimit))
                {
                    snapped = true;
                    return p1;
                }
                else
                {
                    int f0Frame = f0.Frame;

                    if (f0Frame < (f1.Frame - Core.RemoteSendRate * 2))
                    {
                        f0Frame = f1.Frame - Core.RemoteSendRate * 2;
                    }

                    float t = f1.Frame - f0Frame;
                    float d = frame - f0Frame;

                    return Vector3.Lerp(p0, p1, d / t);
                }
            }
        }

        public static Quaternion InterpolateQuaternion(ListExtended<NetworkStorage> frames, int offset, int frame)
        {
            var f0 = frames.First;
            var p0 = f0.Values[offset].Quaternion;

            if (p0 == default(Quaternion))
            {
                p0 = Quaternion.identity;
            }

            if ((frames.Count == 1) || (f0.Frame >= frame))
            {
                return p0;
            }
            else
            {
                var f1 = frames.Next(f0);
                var p1 = f1.Values[offset].Quaternion;

                if (p1 == default(Quaternion))
                {
                    p1 = Quaternion.identity;
                }

                NetAssert.True(f1.Frame > f0.Frame);
                NetAssert.True(f1.Frame > frame);

                int f0Frame = f0.Frame;
                if (f0Frame < (f1.Frame - Core.RemoteSendRate * 2))
                {
                    f0Frame = f1.Frame - Core.RemoteSendRate * 2;
                }

                float t = f1.Frame - f0Frame;
                float d = frame - f0Frame;

                return Quaternion.Lerp(p0, p1, d / t);
            }
        }

        public static Vector3 ExtrapolateVector(Vector3 cpos, Vector3 rpos, Vector3 rvel, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings, ref bool snapped)
        {
            rvel *= AscensionNetwork.FrameDeltaTime;

            float d = System.Math.Min(settings.MaxFrames, (entityFrame + 1) - recievedFrame);
            float t = d / System.Math.Max(1, settings.MaxFrames);

            Vector3 p0 = cpos + (rvel);
            Vector3 p1 = rpos + (rvel * d);

            float sqrMag = (p1 - p0).sqrMagnitude;

            if ((settings.SnapMagnitude > 0) && sqrMag > (settings.SnapMagnitude * settings.SnapMagnitude))
            {
                snapped = true;
                return p1;
            }
            else
            {
                //TODO: implement error tolerance
                //if (rvel.sqrMagnitude < sqrMag) {
                //  return p0;
                //}

                return Vector3.Lerp(p0, p1, t);
            }
        }

        public static Quaternion ExtrapolateQuaternion(Quaternion cquat, Quaternion rquat, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings)
        {
            var r = rquat * Quaternion.Inverse(cquat);
            float d = System.Math.Min(settings.MaxFrames, (entityFrame + 1) - recievedFrame);
            float t = d / (float)System.Math.Max(1, settings.MaxFrames);

            float r_angle;
            Vector3 r_axis;

            r.ToAngleAxis(out r_angle, out r_axis);

            if (float.IsInfinity(r_axis.x) || float.IsNaN(r_axis.x))
            {
                r_angle = 0;
                r_axis = Vector3.right;
            }
            else
            {
                if (r_angle > 180)
                {
                    r_angle -= 360;
                }
            }

            return Quaternion.AngleAxis((r_angle * t) % 360f, r_axis) * cquat;
        }

        public static int SequenceDistance(uint from, uint to, int shift)
        {
            from <<= shift;
            to <<= shift;
            return ((int)(from - to)) >> shift;
        }

        public static int BitsRequired(int number)
        {
            if (number < 0)
            {
                return 32;
            }

            if (number == 0)
            {
                return 1;
            }

            for (int i = 31; i >= 0; --i)
            {
                int b = 1 << i;

                if ((number & b) == b)
                {
                    return i + 1;
                }
            }

            throw new Exception();
        }

        public static int PopCount(ulong value)
        {
            int Count = 0;

            for (int i = 0; i < 32; ++i)
            {
                if ((value & (1UL << i)) != 0)
                {
                    Count += 1;
                }
            }

            return Count;
        }
    #endregion

    #region UDP / Packet Math
    public static bool IsPowerOfTwo(uint x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static bool IsMultipleOf8(uint value)
        {
            return (value > 0) && (((value >> 3) << 3) == value);
        }

        public static bool IsMultipleOf8(int value)
        {
            return (value > 0) && (((value >> 3) << 3) == value);
        }

        public static uint NextPow2(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static int HighBit(uint v)
        {
            if (v == 0)
                return 0;

            int r = 0;

            do
            {
                r += 1;
            } while ((v >>= 1) > 0);

            return r;
        }

        public static int BytesRequired(int bits)
        {
            return (bits + 7) >> 3;
        }

        public static int SeqDistance(uint from, uint to, int shift)
        {
            from <<= shift;
            to <<= shift;
            return ((int)(from - to)) >> shift;
        }

        public static int SeqDistance(ushort from, ushort to, int shift)
        {
            from <<= shift;
            to <<= shift;
            return ((short)(from - to)) >> shift;
        }

        public static uint SeqNext(uint seq, uint mask)
        {
            seq += 1;
            seq &= mask;
            return seq;
        }

        public static ushort SeqNext(ushort seq, ushort mask)
        {
            seq += 1;
            seq &= mask;
            return seq;
        }

        public static ushort SeqPrev(ushort seq, ushort mask)
        {
            seq -= 1;
            seq &= mask;
            return seq;
        }

        public static bool IsSet(uint mask, uint flag)
        {
            return (mask & flag) == flag;
        }

        public static ushort Clamp(ushort value, ushort min, ushort max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static uint Clamp(uint value, uint min, uint max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static byte Clamp(byte value, byte min, byte max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
        #endregion
    }
}
