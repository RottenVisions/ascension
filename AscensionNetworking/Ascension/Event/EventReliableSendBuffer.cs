using System.Collections.Generic;

namespace Ascension.Networking
{
    class EventReliableSendBuffer
    {
        enum State
        {
            Free = 0,
            Send = 1,
            Transit = 2,
            Delivered = 3
        }

        struct Node
        {
            public State State;
            public EventReliable Value;
        }

        int tail;
        int mask;
        int shift;
        int count;

        Node[] nodes;
        SequenceGenerator generator;

        public IEnumerable<EventReliable> Pending
        {
            get
            {
                EventReliable val;

                while (TryNext(out val))
                {
                    yield return val;
                }
            }
        }

        public IEnumerable<EventReliable> Delivered
        {
            get
            {
                EventReliable val;

                while (TryRemove(out val))
                {
                    yield return val;
                }
            }
        }

        public bool Full
        {
            get { return count == nodes.Length; }
        }

        public EventReliableSendBuffer(int windowBits, int sequenceBits)
        {
            nodes = new Node[1 << windowBits];
            shift = 32 - sequenceBits;
            mask = nodes.Length - 1;
            generator = new SequenceGenerator(sequenceBits, uint.MaxValue);
        }

        public bool TryEnqueue(EventReliable value)
        {
            int index = -1;

            if (count == 0)
            {
                index = tail;
            }
            else
            {
                if (count == nodes.Length)
                {
                    return false;
                }

                index = (tail + count) & mask;
            }

            nodes[index].Value = value;
            nodes[index].Value.Sequence = generator.Next();
            nodes[index].State = State.Send;

            count += 1;
            return true;
        }

        public bool TryNext(out EventReliable value)
        {
            for (int i = 0; i < count; ++i)
            {
                int index = (tail + i) & mask;

                if (nodes[index].State == State.Send)
                {
                    nodes[index].State = State.Transit;
                    value = nodes[index].Value;
                    return true;
                }
            }

            value = default(EventReliable);
            return false;
        }

        public void SetDelivered(EventReliable value)
        {
            ChangeState(value, State.Delivered);
        }

        public void SetSend(EventReliable value)
        {
            ChangeState(value, State.Send);
        }

        public bool TryRemove(out EventReliable value)
        {
            if (count > 0 && nodes[tail].State == State.Delivered)
            {
                value = nodes[tail].Value;
                nodes[tail] = default(Node);

                tail += 1;
                tail &= mask;

                count -= 1;
                return true;
            }

            value = default(EventReliable);
            return false;
        }

        void ChangeState(EventReliable value, State state)
        {
            if (count == 0)
            {
                return;
            }

            int distance = SequenceDistance(value.Sequence, nodes[tail].Value.Sequence);
            if (distance < 0 || distance >= count)
            {
                return;
            }

            nodes[(tail + distance) & mask].State = state;
        }

        int SequenceDistance(uint from, uint to)
        {
            from <<= shift;
            to <<= shift;
            return ((int)(from - to)) >> shift;
        }
    }
}