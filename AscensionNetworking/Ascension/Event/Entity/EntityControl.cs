using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public partial class Entity
    {
        public void TakeControl(IMessageRider token)
        {
            if (IsOwner)
            {
                if (HasControl)
                {
                    NetLog.Warn("You already have control of {0}", this);
                }
                else
                {
                    // revoke any existing control
                    RevokeControl(token);

                    // take control locally
                    TakeControlInternal(token);


                    // de-freeze
                    Freeze(false);
                }
            }
            else
            {
                NetLog.Error("Only the owner of {0} can take control of it", this);
            }
        }

        public void TakeControlInternal(IMessageRider token)
        {
            NetAssert.False(Flags & EntityFlags.HAS_CONTROL);

            Flags |= EntityFlags.HAS_CONTROL;

            CommandQueue.Clear();
            CommandSequence = 0;
            CommandLastExecuted = null;

            ControlGainedToken = token;
            ControlLostToken = null;

            // call to serializer
            Serializer.OnControlGained();

            // raise user event
            GlobalEventListenerBase.ControlOfEntityGainedInvoke(UnityObject);

            // call to user behaviours
            foreach (IEntityBehaviour eb in Behaviours)
            {
                if (ReferenceEquals(eb.entity, this.UnityObject))
                {
                    eb.ControlGained();
                }
            }

            Freeze(false);
        }

        public void ReleaseControl(IMessageRider token)
        {
            if (IsOwner)
            {
                if (HasControl)
                {
                    ReleaseControlInternal(token);

                    // un-freeze
                    Freeze(false);
                }
                else
                {
                    NetLog.Warn("You are not controlling {0}", this);
                }
            }
            else
            {
                NetLog.Error("You can not release control of {0}, you are not the owner", this);
            }
        }

        public void ReleaseControlInternal(IMessageRider token)
        {
            NetAssert.True(Flags & EntityFlags.HAS_CONTROL);

            Flags &= ~EntityFlags.HAS_CONTROL;
            CommandQueue.Clear();
            CommandSequence = 0;
            CommandLastExecuted = null;

            ControlLostToken = token;
            ControlGainedToken = null;

            // call to serializer
            Serializer.OnControlLost();

            // call to user behaviours
            foreach (IEntityBehaviour eb in Behaviours)
            {
                if (ReferenceEquals(eb.entity, this.UnityObject))
                {
                    eb.ControlLost();
                }
            }

            // call user event
            GlobalEventListenerBase.ControlOfEntityLostInvoke(UnityObject);

            // de-freeze
            Freeze(false);
        }

        public void AssignControl(Connection connection, IMessageRider token)
        {
            if (IsOwner)
            {
                if (HasControl)
                {
                    ReleaseControl(token);
                }

                EntityProxy proxy;

                CommandLastExecuted = null;
                CommandSequence = 0;
                CommandQueue.Clear();

                Controller = connection;
                Controller.controlling.Add(this);
                Controller.entityChannel.CreateOnRemote(this, out proxy);
                Controller.entityChannel.ForceSync(this);

                // set token 
                proxy.ControlTokenLost = null;
                proxy.ControlTokenGained = token;

                Freeze(false);
            }
            else
            {
                NetLog.Error("You can not assign control of {0}, you are not the owner", this);
            }
        }

        public void RevokeControl(IMessageRider token)
        {
            if (IsOwner)
            {
                if (Controller)
                {
                    EntityProxy proxy;

                    // force a replication of this
                    Controller.controlling.Remove(this);
                    Controller.entityChannel.ForceSync(this, out proxy);
                    Controller = null;

                    // clear out everything
                    CommandLastExecuted = null;
                    CommandSequence = 0;
                    CommandQueue.Clear();

                    // set token
                    if (proxy != null)
                    {
                        proxy.ControlTokenLost = token;
                        proxy.ControlTokenGained = null;
                    }
                }

                Freeze(false);
            }
            else
            {
                NetLog.Error("You can not revoke control of {0}, you are not the owner", this);
                return;
            }
        }

        public bool QueueInput(Command cmd)
        {
            if (canQueueCommands)
            {
                NetAssert.True(HasControl);

                if (CommandQueue.Count < Core.Config.commandQueueSize)
                {
                    cmd.ServerFrame = Core.ServerFrame;
                    cmd.Sequence = CommandSequence = NetMath.SeqNext(CommandSequence, Command.SEQ_MASK);
                }
                else
                {
                    NetLog.Error("Input queue for {0} is full", this);
                    return false;
                }

                CommandQueue.AddLast(cmd);
                return true;
            }
            else
            {
                NetLog.Error("You can only queue commands to the host in the 'SimulateController' callback");
                return false;
            }
        }
    }
}
