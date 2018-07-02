#define DEBUG
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Ascension.Networking.Sockets;
using UE = UnityEngine;

namespace Ascension.Networking
{
    partial class NetworkState : IEntitySerializer
    {
        TypeId IEntitySerializer.TypeId
        {
            get { return Meta.TypeId; }
        }

        void IEntitySerializer.OnRender()
        {
            for (int i = 0; i < Meta.OnRender.Count; ++i)
            {
                var p = Meta.OnRender[i];
                p.Property.OnRender(Objects[p.OffsetObjects]);
            }
        }

        void IEntitySerializer.OnInitialized()
        {
            NetworkStorage storage;

            storage = AllocateStorage();
            storage.Frame = Entity.IsOwner ? Core.Frame : -1;

            Frames.AddLast(storage);

            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                var p = Meta.Properties[i];
                p.Property.OnInit(Objects[p.OffsetObjects]);
            }
        }

        void IEntitySerializer.OnCreated(Entity entity)
        {
            Entity = entity;
        }

        void IEntitySerializer.OnParentChanging(Entity newParent, Entity oldParent)
        {
            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                var p = Meta.Properties[i];
                p.Property.OnParentChanged(Objects[p.OffsetObjects], newParent, oldParent);
            }
        }

        void IEntitySerializer.OnSimulateBefore()
        {
            if (Entity.IsOwner || Entity.HasPredictedControl)
            {
                Frames.First.Frame = Core.Frame;
            }
            else
            {
                while ((Frames.Count > 1) && (Entity.Frame >= Frames.Next(Frames.First).Frame))
                {
                    // combine changed properties
                    Frames.Next(Frames.First).Combine(Frames.First);

                    // free it
                    FreeStorage(Frames.RemoveFirst());
                }
            }

            int count = Meta.OnSimulateBefore.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; ++i)
                {
                    var p = Meta.OnSimulateBefore[i];
                    p.Property.OnSimulateBefore(Objects[p.OffsetObjects]);
                }
            }

            InvokeCallbacks();
        }

        void IEntitySerializer.OnSimulateAfter()
        {
            int count = Meta.OnSimulateAfter.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; ++i)
                {
                    var p = Meta.OnSimulateAfter[i];
                    p.Property.OnSimulateAfter(Objects[p.OffsetObjects]);
                }
            }

            InvokeCallbacks();
        }

        void IEntitySerializer.OnControlGained()
        {
            while (Frames.Count > 1)
            {
                // compact all changes into last frame
                Frames.Last.Combine(Frames.First);

                // remove First frame
                FreeStorage(Frames.RemoveFirst());
            }
        }

        void IEntitySerializer.OnControlLost()
        {
            Frames.First.Frame = Entity.Frame;
        }

        BitSet IEntitySerializer.GetDefaultMask()
        {
            return PropertyDefaultMask;
        }

        BitSet IEntitySerializer.GetFilter(Connection connection, EntityProxy proxy)
        {
            if (Entity.IsController(connection))
            {
                return Meta.Filters[31];
            }

            return Meta.Filters[30];
        }

        void IEntitySerializer.DebugInfo()
        {
#if DEBUG
            if (AscensionNetworkInternal.DebugDrawer != null)
            {
                AscensionNetworkInternal.DebugDrawer.LabelBold("");
                AscensionNetworkInternal.DebugDrawer.LabelBold("State Info");
                AscensionNetworkInternal.DebugDrawer.LabelField("Type", Factory.GetFactory(Meta.TypeId).TypeObject);
                AscensionNetworkInternal.DebugDrawer.LabelField("Type Id", Meta.TypeId);

                AscensionNetworkInternal.DebugDrawer.LabelBold("");
                AscensionNetworkInternal.DebugDrawer.LabelBold("State Properties");

                for (int i = 0; i < Meta.Properties.Length; ++i)
                {
                    var pi = Meta.Properties[i];

                    string label = pi.Paths.NullOrEmpty() ? pi.Property.PropertyName : FixArrayIndices(pi.Paths[pi.Paths.Length - 1], pi.Indices);
                    object value = pi.Property.DebugValue(Objects[pi.OffsetObjects], Storage);

                    if (!Entity.IsOwner)
                    {
                        EntityProxy proxy;

                        if (Entity.Source.entityChannel.TryFindProxy(Entity, out proxy))
                        {
                            label = "(" + proxy.PropertyPriority[i].PropertyUpdated + ") " + label;
                        }
                    }

                    if (value != null)
                    {
                        AscensionNetworkInternal.DebugDrawer.LabelField(label, value.ToString());
                    }
                    else
                    {
                        AscensionNetworkInternal.DebugDrawer.LabelField(label, "N/A");
                    }
                }
            }
#endif
        }

        string FixArrayIndices(string path, int[] indices)
        {
            Regex r = new Regex(@"\[\]");

            for (int i = 0; i < indices.Length; ++i)
            {
                path = r.Replace(path, "[" + indices[i] + "]", 1);
            }

            return path;
        }

        void IEntitySerializer.InitProxy(EntityProxy p)
        {
            p.PropertyPriority = new Priority[Meta.CountProperties];

            for (int i = 0; i < p.PropertyPriority.Length; ++i)
            {
                p.PropertyPriority[i].PropertyIndex = i;
            }
        }

        int IEntitySerializer.Pack(Connection connection, Packet stream, EntityProxyEnvelope env)
        {
            int propertyCount = 0;

            BitSet filter = ((IEntitySerializer)this).GetFilter(connection, env.Proxy);

            Priority[] tempPriority = Meta.PropertiesTempPriority;
            Priority[] proxyPriority = env.Proxy.PropertyPriority;

            for (int i = 0; i < proxyPriority.Length; ++i)
            {
                NetAssert.True(proxyPriority[i].PropertyIndex == i);

                // if this property is set both in our filter and the proxy mask we can consider it for sending
                if (filter.IsSet(i) && env.Proxy.IsSet(i))
                {
                    // increment priority for this property
                    proxyPriority[i].PropertyPriority += Meta.Properties[i].Property.PropertyPriority;
                    proxyPriority[i].PropertyPriority = UE.Mathf.Clamp(proxyPriority[i].PropertyPriority, 0, Core.Config.maxPropertyPriority);

                    // copy to our temp array
                    tempPriority[propertyCount] = proxyPriority[i];

                    // increment temp count
                    propertyCount += 1;
                }
            }

            // sort temp array based on priority
            Array.Sort<Priority>(tempPriority, 0, propertyCount, Priority.Comparer.Instance);

            // write into stream
            PackProperties(connection, stream, env, tempPriority, propertyCount);

            for (int i = 0; i < env.Written.Count; ++i)
            {
                Priority p = env.Written[i];

                // clear priority for written property
                env.Proxy.PropertyPriority[p.PropertyIndex].PropertyPriority = 0;

                // clear mask for it
                env.Proxy.Clear(p.PropertyIndex);
            }

            return env.Written.Count;
        }

        void PackProperties(Connection connection, Packet packet, EntityProxyEnvelope env, Priority[] priority, int count)
        {
            int propertyCountPtr = packet.Position;
            packet.WriteByte(0, Meta.PacketMaxPropertiesBits);

            // how many bits can we write at the most
            int bits = System.Math.Min(Meta.PacketMaxBits, packet.Size - packet.Position);

            for (int i = 0; i < count; ++i)
            {
                // this means we can even fit another property id
                if (bits <= Meta.PropertyIdBits)
                {
                    break;
                }

                // we have written enough properties
                if (env.Written.Count == Meta.PacketMaxProperties)
                {
                    break;
                }

                Priority p = priority[i];
                NetworkPropertyInfo pi = Meta.Properties[p.PropertyIndex];

                if (p.PropertyPriority == 0)
                {
                    break;
                }

                int b = Meta.PropertyIdBits + pi.Property.BitCount(Objects[pi.OffsetObjects]);
                int ptr = packet.Position;

                if (bits >= b)
                {
                    // write property id
                    packet.WriteInt(p.PropertyIndex, Meta.PropertyIdBits);

                    if (pi.Property.Write(connection, Objects[pi.OffsetObjects], Storage, packet))
                    {

#if DEBUG
                        int totalBits = packet.Position - ptr;
                        if (totalBits != b)
                        {
                            //NetLog.Warn("Property of type {0} did not write the correct amount of bits, written: {1}, expected: {2}", pi.Property, totalBits, b);
                        }
#endif

                        if (packet.Overflowing)
                        {
                            packet.Position = ptr;
                            break;
                        }

                        // use up bits
                        bits -= b;

                        // add to written list
                        env.Written.Add(p);
                    }
                    else
                    {
                        // reset position
                        packet.Position = ptr;
                    }
                }
            }

            // gotta be less then 256
            NetAssert.True(env.Written.Count <= Meta.PacketMaxProperties);

            // write the amount of properties
            Packet.WriteByteAt(packet.ByteBuffer, propertyCountPtr, Meta.PacketMaxPropertiesBits, (byte)env.Written.Count);
        }

        void IEntitySerializer.Read(Connection connection, Packet packet, int frame)
        {
            int count = packet.ReadByte(Meta.PacketMaxPropertiesBits);
            var storage = default(NetworkStorage);

            if (Entity.HasPredictedControl)
            {
                NetAssert.True(Frames.Count == 1);

                storage = Frames.First;
                storage.Frame = Core.Frame;
            }
            else
            {
                if (Frames.First.Frame == -1)
                {
                    NetAssert.True(Frames.Count == 1);

                    storage = Frames.First;
                    storage.Frame = frame;
                }
                else
                {
                    storage = DuplicateStorage(Frames.Last);
                    storage.Frame = frame;
                    storage.ClearAll();

                    // tell the properties that need to know about this
                    for (int i = 0; i < Meta.OnFrameCloned.Count; ++i)
                    {
                        // grab property info
                        var pi = Meta.OnFrameCloned[i];

                        // invoke callback
                        pi.Property.OnFrameCloned(Objects[pi.OffsetObjects], storage);
                    }

                    Frames.AddLast(storage);
                }
            }

            //fixes bug #224
            //IState.SetTransforms to replace NetworkTransform.SetTransforms, 
            //this new methods works around the issue of position snapping for 
            //entities when their position updates are delayed.

            //NetworkTransform.ChangeTransform to replace the previous NetworkTransform.SetTransforms 
            //for changing the transform target for interpolation after it's been set once.
            if (Entity.HasControl && !Entity.HasPredictedControl && !Entity.IsOwner)
            {
                for (int i = 0; i < Meta.Properties.Length; ++i)
                {
                    var pi = Meta.Properties[i];
                    if (pi.Property.ToController == false)
                    {
                        // calculate property index
                        int index = Objects[pi.OffsetObjects][pi.Property];

                        // copy value from latest frame
                        storage.Values[index] = Frames.First.Values[index];
                    }
                }
            }

            while (--count >= 0)
            {
                var propertyIndex = packet.ReadInt(Meta.PropertyIdBits);
                var propertyInfo = Meta.Properties[propertyIndex];

                if (!Entity.IsOwner)
                {
                    EntityProxy proxy;

                    if (Entity.Source.entityChannel.TryFindProxy(Entity, out proxy))
                    {
                        proxy.PropertyPriority[propertyIndex].PropertyUpdated = frame;
                    }
                }

                // make sure this is the correct one
                NetAssert.True(propertyIndex == Objects[propertyInfo.OffsetObjects].OffsetProperties + propertyInfo.Property.OffsetProperties);

                // read data into frame
                propertyInfo.Property.Read(connection, Objects[propertyInfo.OffsetObjects], storage, packet);

                // set changed flag
                storage.Set(propertyIndex);
            }
        }
    }
}
