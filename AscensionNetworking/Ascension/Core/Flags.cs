namespace Ascension.Networking
{
    public struct EntityFlags
    {
        public static readonly EntityFlags ZERO = new EntityFlags(0);
        public static readonly EntityFlags HAS_CONTROL = new EntityFlags(1);
        public static readonly EntityFlags PERSIST_ON_LOAD = new EntityFlags(2);
        public static readonly EntityFlags ATTACHED = new EntityFlags(4);
        public static readonly EntityFlags CONTROLLER_LOCAL_PREDICTION = new EntityFlags(8);
        public static readonly EntityFlags SCENE_OBJECT = new EntityFlags(16);

        readonly int bits;

        public bool IsZero
        {
            get { return bits == 0; }
        }

        EntityFlags(int val)
        {
            bits = val;
        }

        public override int GetHashCode()
        {
            return bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is EntityFlags)
            {
                return bits == ((EntityFlags)obj).bits;
            }

            return false;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            sb.Append("EntityFlags");


            if ((bits & 1) == 1)
            {
                sb.Append(" HAS_CONTROL");
            }
            if ((bits & 2) == 2)
            {
                sb.Append(" PERSIST_ON_LOAD");
            }
            if ((bits & 4) == 4)
            {
                sb.Append(" ATTACHED");
            }
            if ((bits & 8) == 8)
            {
                sb.Append(" CONTROLLER_LOCAL_PREDICTION");
            }
            if ((bits & 16) == 16)
            {
                sb.Append(" SCENE_OBJECT");
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static implicit operator bool (EntityFlags a)
        {
            return a.bits != 0;
        }

        public static explicit operator int (EntityFlags a)
        {
            return a.bits;
        }

        public static explicit operator EntityFlags(int a)
        {
            return new EntityFlags(a);
        }

        public static EntityFlags operator &(EntityFlags a, EntityFlags b)
        {
            return new EntityFlags(a.bits & b.bits);
        }

        public static EntityFlags operator |(EntityFlags a, EntityFlags b)
        {
            return new EntityFlags(a.bits | b.bits);
        }

        public static EntityFlags operator ^(EntityFlags a, EntityFlags b)
        {
            return new EntityFlags(a.bits ^ b.bits);
        }

        public static EntityFlags operator ~(EntityFlags a)
        {
            return new EntityFlags(~a.bits);
        }

        public static bool operator ==(EntityFlags a, EntityFlags b)
        {
            return a.bits == b.bits;
        }

        public static bool operator !=(EntityFlags a, EntityFlags b)
        {
            return a.bits != b.bits;
        }
    }

    public struct InstantiateFlags
    {
        public static readonly InstantiateFlags ZERO = new InstantiateFlags(0);
        public static readonly InstantiateFlags TAKE_CONTROL = new InstantiateFlags(1);
        public static readonly InstantiateFlags ASSIGN_CONTROL = new InstantiateFlags(2);

        readonly int bits;

        public bool IsZero
        {
            get { return bits == 0; }
        }

        InstantiateFlags(int val)
        {
            bits = val;
        }

        public override int GetHashCode()
        {
            return bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is InstantiateFlags)
            {
                return bits == ((InstantiateFlags)obj).bits;
            }

            return false;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            sb.Append("InstantiateFlags");


            if ((bits & 1) == 1)
            {
                sb.Append(" TAKE_CONTROL");
            }
            if ((bits & 2) == 2)
            {
                sb.Append(" ASSIGN_CONTROL");
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static implicit operator bool (InstantiateFlags a)
        {
            return a.bits != 0;
        }

        public static explicit operator int (InstantiateFlags a)
        {
            return a.bits;
        }

        public static explicit operator InstantiateFlags(int a)
        {
            return new InstantiateFlags(a);
        }

        public static InstantiateFlags operator &(InstantiateFlags a, InstantiateFlags b)
        {
            return new InstantiateFlags(a.bits & b.bits);
        }

        public static InstantiateFlags operator |(InstantiateFlags a, InstantiateFlags b)
        {
            return new InstantiateFlags(a.bits | b.bits);
        }

        public static InstantiateFlags operator ^(InstantiateFlags a, InstantiateFlags b)
        {
            return new InstantiateFlags(a.bits ^ b.bits);
        }

        public static InstantiateFlags operator ~(InstantiateFlags a)
        {
            return new InstantiateFlags(~a.bits);
        }

        public static bool operator ==(InstantiateFlags a, InstantiateFlags b)
        {
            return a.bits == b.bits;
        }

        public static bool operator !=(InstantiateFlags a, InstantiateFlags b)
        {
            return a.bits != b.bits;
        }
    }

    public struct ProxyFlags
    {
        public static readonly ProxyFlags ZERO = new ProxyFlags(0);
        public static readonly ProxyFlags CREATE_REQUESTED = new ProxyFlags(1);
        public static readonly ProxyFlags CREATE_DONE = new ProxyFlags(2);
        public static readonly ProxyFlags DESTROY_REQUESTED = new ProxyFlags(4);
        public static readonly ProxyFlags DESTROY_PENDING = new ProxyFlags(8);
        public static readonly ProxyFlags DESTROY_IGNORE = new ProxyFlags(16);
        public static readonly ProxyFlags IDLE = new ProxyFlags(32);
        public static readonly ProxyFlags FORCE_SYNC = new ProxyFlags(64);

        readonly int bits;

        public bool IsZero
        {
            get { return bits == 0; }
        }

        ProxyFlags(int val)
        {
            bits = val;
        }

        public override int GetHashCode()
        {
            return bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is ProxyFlags)
            {
                return bits == ((ProxyFlags)obj).bits;
            }

            return false;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            sb.Append("ProxyFlags");


            if ((bits & 1) == 1)
            {
                sb.Append(" CREATE_REQUESTED");
            }
            if ((bits & 2) == 2)
            {
                sb.Append(" CREATE_DONE");
            }
            if ((bits & 4) == 4)
            {
                sb.Append(" DESTROY_REQUESTED");
            }
            if ((bits & 8) == 8)
            {
                sb.Append(" DESTROY_PENDING");
            }
            if ((bits & 16) == 16)
            {
                sb.Append(" DESTROY_IGNORE");
            }
            if ((bits & 32) == 32)
            {
                sb.Append(" IDLE");
            }
            if ((bits & 64) == 64)
            {
                sb.Append(" FORCE_SYNC");
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static implicit operator bool (ProxyFlags a)
        {
            return a.bits != 0;
        }

        public static explicit operator int (ProxyFlags a)
        {
            return a.bits;
        }

        public static explicit operator ProxyFlags(int a)
        {
            return new ProxyFlags(a);
        }

        public static ProxyFlags operator &(ProxyFlags a, ProxyFlags b)
        {
            return new ProxyFlags(a.bits & b.bits);
        }

        public static ProxyFlags operator |(ProxyFlags a, ProxyFlags b)
        {
            return new ProxyFlags(a.bits | b.bits);
        }

        public static ProxyFlags operator ^(ProxyFlags a, ProxyFlags b)
        {
            return new ProxyFlags(a.bits ^ b.bits);
        }

        public static ProxyFlags operator ~(ProxyFlags a)
        {
            return new ProxyFlags(~a.bits);
        }

        public static bool operator ==(ProxyFlags a, ProxyFlags b)
        {
            return a.bits == b.bits;
        }

        public static bool operator !=(ProxyFlags a, ProxyFlags b)
        {
            return a.bits != b.bits;
        }
    }

    public struct CommandFlags
    {
        public static readonly CommandFlags ZERO = new CommandFlags(0);
        public static readonly CommandFlags HAS_EXECUTED = new CommandFlags(1);
        public static readonly CommandFlags SEND_STATE = new CommandFlags(2);
        public static readonly CommandFlags SEND_STATE_PERFORMED = new CommandFlags(4);
        public static readonly CommandFlags CORRECTION_RECEIVED = new CommandFlags(8);
        public static readonly CommandFlags DISPOSE = new CommandFlags(16);
        public static readonly CommandFlags MISSING = new CommandFlags(32);

        readonly int bits;

        public bool IsZero
        {
            get { return bits == 0; }
        }

        CommandFlags(int val)
        {
            bits = val;
        }

        public override int GetHashCode()
        {
            return bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is CommandFlags)
            {
                return bits == ((CommandFlags)obj).bits;
            }

            return false;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            sb.Append("CommandFlags");


            if ((bits & 1) == 1)
            {
                sb.Append(" HAS_EXECUTED");
            }
            if ((bits & 2) == 2)
            {
                sb.Append(" SEND_STATE");
            }
            if ((bits & 4) == 4)
            {
                sb.Append(" SEND_STATE_PERFORMED");
            }
            if ((bits & 8) == 8)
            {
                sb.Append(" CORRECTION_RECEIVED");
            }
            if ((bits & 16) == 16)
            {
                sb.Append(" DISPOSE");
            }
            if ((bits & 32) == 32)
            {
                sb.Append(" MISSING");
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static implicit operator bool (CommandFlags a)
        {
            return a.bits != 0;
        }

        public static explicit operator int (CommandFlags a)
        {
            return a.bits;
        }

        public static explicit operator CommandFlags(int a)
        {
            return new CommandFlags(a);
        }

        public static CommandFlags operator &(CommandFlags a, CommandFlags b)
        {
            return new CommandFlags(a.bits & b.bits);
        }

        public static CommandFlags operator |(CommandFlags a, CommandFlags b)
        {
            return new CommandFlags(a.bits | b.bits);
        }

        public static CommandFlags operator ^(CommandFlags a, CommandFlags b)
        {
            return new CommandFlags(a.bits ^ b.bits);
        }

        public static CommandFlags operator ~(CommandFlags a)
        {
            return new CommandFlags(~a.bits);
        }

        public static bool operator ==(CommandFlags a, CommandFlags b)
        {
            return a.bits == b.bits;
        }

        public static bool operator !=(CommandFlags a, CommandFlags b)
        {
            return a.bits != b.bits;
        }
    }

}