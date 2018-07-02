using System.Runtime.InteropServices;

namespace Ascension.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkTrigger
    {
        [FieldOffset(0)]
        public int Frame;

        [FieldOffset(4)]
        public int History;

        public void Update(int frame, bool set)
        {
            if (frame > this.Frame)
            {
                // get diff
                int diff = frame - this.Frame;

                // update history
                this.History = (diff < 32) ? (this.History << diff) : 0;

                if (set)
                {
                    this.History |= 1;
                }

                this.Frame = frame;
            }
            else if (frame == this.Frame)
            {
                if (set)
                {
                    this.History |= 1;
                }
            }
        }

        public static bool operator ==(NetworkTrigger a, NetworkTrigger b)
        {
            return a.Frame == b.Frame && a.History == b.History;
        }

        public static bool operator !=(NetworkTrigger a, NetworkTrigger b)
        {
            return a.Frame != b.Frame || a.History != b.History;
        }
    }
}
