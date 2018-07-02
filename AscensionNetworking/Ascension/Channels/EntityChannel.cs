using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ascension.Networking
{
    public partial class EntityChannel : Channel
    {
        public EntityLookup _outgoingLookup;
        public EntityLookup _incommingLookup;

        public Dictionary<NetworkId, EntityProxy> _outgoingDict;
        public Dictionary<NetworkId, EntityProxy> _incommingDict;

        private List<EntityProxy> _prioritized;

        public EntityChannel()
        {
            _outgoingDict = new Dictionary<NetworkId, EntityProxy>(2048, NetworkId.EqualityComparer.Instance);
            _incommingDict = new Dictionary<NetworkId, EntityProxy>(2048, NetworkId.EqualityComparer.Instance);

            _outgoingLookup = new EntityLookup(_outgoingDict);
            _incommingLookup = new EntityLookup(_incommingDict);

            _prioritized = new List<EntityProxy>();
        }

        public void ForceSync(Entity en)
        {
            EntityProxy proxy;
            ForceSync(en, out proxy);
        }

        public void ForceSync(Entity en, out EntityProxy proxy)
        {
            if (_outgoingDict.TryGetValue(en.NetworkId, out proxy))
            {
                proxy.Flags |= ProxyFlags.FORCE_SYNC;
                proxy.Flags &= ~ProxyFlags.IDLE;
            }
        }

        public bool TryFindProxy(Entity en, out EntityProxy proxy)
        {
            return _incommingDict.TryGetValue(en.NetworkId, out proxy) ||
                   _outgoingDict.TryGetValue(en.NetworkId, out proxy);
        }

        public void SetIdle(Entity entity, bool idle)
        {
            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                if (idle)
                {
                    proxy.Flags |= ProxyFlags.IDLE;
                }
                else
                {
                    proxy.Flags &= ~ProxyFlags.IDLE;
                }
            }
        }

        public void SetScope(Entity entity, bool inScope)
        {
            if (Core.Config.scopeMode == ScopeMode.Automatic)
            {
                NetLog.Error("SetScope has no effect when Scope Mode is set to Automatic");
                return;
            }

            if (ReferenceEquals(entity.Source, connection))
            {
                return;
            }

            if (inScope)
            {
                if (_incommingDict.ContainsKey(entity.NetworkId))
                {
                    return;
                }

                EntityProxy proxy;

                if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
                {
                    if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED)
                    {
                        if (proxy.Flags & ProxyFlags.DESTROY_PENDING)
                        {
                            proxy.Flags |= ProxyFlags.DESTROY_IGNORE;
                        }
                        else
                        {
                            proxy.Flags &= ~ProxyFlags.DESTROY_IGNORE;
                            proxy.Flags &= ~ProxyFlags.DESTROY_REQUESTED;
                        }
                    }
                }
                else
                {
                    CreateOnRemote(entity);
                }
            }
            else
            {
                if (_outgoingDict.ContainsKey(entity.NetworkId))
                {
                    DestroyOnRemote(entity);
                }
            }
        }

        public bool ExistsOnRemote(Entity entity)
        {
            if (entity == null)
            {
                return false;
            }
            if (_incommingDict.ContainsKey(entity.NetworkId))
            {
                return true;
            }

            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                return (proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED);
            }

            return false;
        }

        public ExistsResult ExistsOnRemote(Entity entity, bool allowMaybe)
        {
            if (entity == null)
            {
                return ExistsResult.No;
            }
            if (_incommingDict.ContainsKey(entity.NetworkId))
            {
                return ExistsResult.Yes;
            }

            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                if ((proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED))
                {
                    return ExistsResult.Yes;
                }

                if (allowMaybe)
                {
                    return ExistsResult.Maybe;
                }
            }

            return ExistsResult.No;
        }

        public bool MightExistOnRemote(Entity entity)
        {
            return _incommingDict.ContainsKey(entity.NetworkId) || _outgoingDict.ContainsKey(entity.NetworkId);
        }

        public void DestroyOnRemote(Entity entity)
        {
            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                // if we dont have any pending sends for this and we have not created it;
                if (proxy.Envelopes.Count == 0 && !(proxy.Flags & ProxyFlags.CREATE_DONE))
                {
                    DestroyOutgoingProxy(proxy);

                }
                else
                {
                    proxy.Flags |= ProxyFlags.DESTROY_REQUESTED;
                    proxy.Flags &= ~ProxyFlags.IDLE;
                }
            }
        }

        public void CreateOnRemote(Entity entity)
        {
            EntityProxy proxy;
            CreateOnRemote(entity, out proxy);
        }

        public void CreateOnRemote(Entity entity, out EntityProxy proxy)
        {
            if (_incommingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                return;
            }
            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                return;
            }

            proxy = entity.CreateProxy();
            proxy.NetworkId = entity.NetworkId;
            proxy.Flags = ProxyFlags.CREATE_REQUESTED;
            proxy.Connection = connection;

            _outgoingDict.Add(proxy.NetworkId, proxy);

            NetLog.Debug("Created {0} on {1}", entity, connection);
        }

        public float GetPriority(Entity entity)
        {
            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
            {
                return proxy.Priority;
            }

            return float.NegativeInfinity;
        }

        public override void Pack(Packet packet)
        {
            int startPos = packet.Position;

            // always clear before starting
            _prioritized.Clear();

            foreach (EntityProxy proxy in _outgoingDict.Values)
            {
                if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED)
                {
                    if (proxy.Flags & ProxyFlags.DESTROY_PENDING)
                    {
                        continue;
                    }

                    proxy.ClearAll();
                    proxy.Priority = 1 << 17;
                }
                else
                {
                    if (proxy.Entity.IsFrozen)
                    {
                        continue;
                    }

                    // check update rate of this entity
                    if ((packet.Number % proxy.Entity.UpdateRate) != 0)
                    {
                        continue;
                    }

                    // meep
                    if (proxy.Envelopes.Count >= 256)
                    {
                        NetLog.Error("Envelopes for {0} to {1} full", proxy, connection);
                        continue;
                    }

                    // if this connection is loading a map dont send any creates or state updates
                    if (proxy.Entity.UnityObject.alwaysProxy == false)
                    {
                        if (connection.IsLoadingMap || SceneLoader.IsLoading ||
                            (connection.canReceiveEntities == false))
                        {
                            //TODO: LOOK HERE IF WE HAVE ISSUES
                            //Debug.Log(connection.remoteSceneLoading.State + " " + connection.remoteSceneLoading.Scene + " " + Core.LocalSceneLoading.Scene);
                            //Debug.Log(connection.IsLoadingMap + " " + SceneLoader.IsLoading + " " + (connection.canReceiveEntities == false));
                            //NetLog.Info("R-STATE:{0}, R-SCENE:{1}, L-SCENE:{2}", connection.remoteSceneLoading.State, connection.remoteSceneLoading.Scene, Core.LocalSceneLoading.Scene);
                            continue;
                        }
                    }

                    if (proxy.Flags & ProxyFlags.FORCE_SYNC)
                    {
                        if (proxy.Entity.ReplicationFilter.AllowReplicationTo(connection))
                        {
                            proxy.Priority = 1 << 20;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (proxy.Flags & ProxyFlags.CREATE_DONE)
                        {
                            if (proxy.IsZero)
                            {
                                continue;
                            }
                            // check for idle flag
                            if (proxy.Flags & ProxyFlags.IDLE)
                            {
                                continue;
                            }
                            proxy.Priority = proxy.Priority +
                                             proxy.Entity.PriorityCalculator.CalculateStatePriority(connection,
                                                 proxy.Skipped);
                            proxy.Priority = Mathf.Clamp(proxy.Priority, 0, Mathf.Min(1 << 16, Core.Config.maxEntityPriority));
                        }
                        else
                        {
                            if ((proxy.Entity.IsFrozen == false) || proxy.Entity.AllowFirstReplicationWhenFrozen)
                            {
                                if (proxy.Entity.ReplicationFilter.AllowReplicationTo(connection))
                                {
                                    proxy.Priority = 1 << 18;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }

                // if this is the controller give it the max priority
                if (proxy.Entity.IsController(connection))
                {
                    proxy.Priority = 1 << 19;
                }
                _prioritized.Add(proxy);
            }

            if (_prioritized.Count > 0)
            {
                try
                {
                    _prioritized.Sort(EntityProxy.PriorityComparer.Instance);

                    // write as many proxies into the packet as possible
                    int failCount = 0;

                    for (int i = 0; i < _prioritized.Count; ++i)
                    {
                        if ((_prioritized[i].Priority <= 0) || (failCount >= 2))
                        {
                            _prioritized[i].Skipped += 1;
                        }
                        else
                        {
                            switch (PackUpdate(packet, _prioritized[i]))
                            {
                                case -1:
                                    failCount += 1;
                                    _prioritized[i].Skipped += 1;
                                    break;

                                case 0:
                                    _prioritized[i].Skipped += 1;
                                    break;

                                case +1:
                                    _prioritized[i].Skipped = 0;
                                    _prioritized[i].Priority = 0;
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    _prioritized.Clear();
                }
            }
            packet.WriteStopMarker();
            packet.Stats.StateBits = packet.Position - startPos;
        }

        public override void Read(Packet packet)
        {
            int startPtr = packet.Position;

            // unpack all of our data
            while (packet.CanRead())
            {
                if (ReadUpdate(packet) == false)
                {
                    break;
                }
            }

            packet.Stats.StateBits = packet.Position - startPtr;
        }

        public override void Lost(Packet packet)
        {
            while (packet.EntityUpdates.Count > 0)
            {
                var env = packet.EntityUpdates.Dequeue();
                var pending = env.Proxy.Envelopes.Dequeue();

                //NetLog.Error("LOST ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
                //NetAssert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
                //NetAssert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", envNumber, pendingNumber));

                // copy back all priorities
                ApplyPropertyPriorities(env);

                // push skipped count up one
                env.Proxy.Skipped += 1;

                // if this was a forced sync, set flag on proxy again
                if (env.Flags & ProxyFlags.FORCE_SYNC)
                {
                    env.Proxy.Flags |= ProxyFlags.FORCE_SYNC;
                }

                // if we failed to destroy this clear destroying flag
                if (env.Flags & ProxyFlags.DESTROY_PENDING)
                {
                    NetAssert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);
                    env.Proxy.Flags &= ~ProxyFlags.DESTROY_PENDING;
                }

                env.Dispose();
            }
        }

        public override void Delivered(Packet packet)
        {
            while (packet.EntityUpdates.Count > 0)
            {
                var env = packet.EntityUpdates.Dequeue();
                var pending = env.Proxy.Envelopes.Dequeue();

                //NetLog.Info("DELIVERED ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
                //NetAssert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
                //NetAssert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", envNumber, pendingNumber));

                if (env.Flags & ProxyFlags.DESTROY_PENDING)
                {
                    NetAssert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);

                    // delete proxy
                    DestroyOutgoingProxy(env.Proxy);
                }
                else if (env.Flags & ProxyFlags.CREATE_REQUESTED)
                {
                    // if this token has been sent, clear it
                    if (ReferenceEquals(env.ControlTokenGained, env.Proxy.ControlTokenGained))
                    {
                        env.Proxy.ControlTokenGained = null;
                    }

                    // clear out request / progress for create
                    env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;

                    // set create done
                    env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
                }

                env.Dispose();
            }
        }

        public override void Disconnected()
        {
            foreach (EntityProxy proxy in _outgoingDict.Values.ToArray())
            {
                if (proxy)
                {
                    DestroyOutgoingProxy(proxy);
                }
            }

            foreach (EntityProxy proxy in _incommingDict.Values.ToArray())
            {
                if (proxy)
                {
                    DestroyIncommingProxy(proxy, null);
                }
            }
        }

        public int GetSkippedUpdates(Entity en)
        {
            EntityProxy proxy;

            if (_outgoingDict.TryGetValue(en.NetworkId, out proxy))
            {
                return proxy.Skipped;
            }

            return -1;
        }

        private void ApplyPropertyPriorities(EntityProxyEnvelope env)
        {
            for (int i = 0; i < env.Written.Count; ++i)
            {
                Priority p = env.Written[i];

                // set flag for sending this property again
                env.Proxy.Set(p.PropertyIndex);

                // increment priority
                env.Proxy.PropertyPriority[p.PropertyIndex].PropertyPriority += p.PropertyPriority;
            }
        }

        private int PackUpdate(Packet packet, EntityProxy proxy)
        {
            int pos = packet.Position;
            int packCount = 0;

            EntityProxyEnvelope env = proxy.CreateEnvelope();

            packet.WriteBool(true);
            packet.WriteNetworkId(proxy.NetworkId);

            if (packet.WriteBool(proxy.Entity.IsController(connection)))
            {
                packet.WriteToken(proxy.ControlTokenGained);
                proxy.ControlTokenLost = null;
            }
            else
            {
                packet.WriteToken(proxy.ControlTokenLost);
                proxy.ControlTokenGained = null;
            }

            if (packet.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED))
            {
                packet.WriteToken(proxy.Entity.DetachToken);
            }
            else
            {
                // data for first packet
                if (packet.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED))
                {
                    packet.WriteToken(proxy.Entity.AttachToken);

                    packet.WritePrefabId(proxy.Entity.PrefabId);
                    packet.WriteTypeId(proxy.Entity.Serializer.TypeId);

                    packet.WriteVector3(proxy.Entity.UnityObject.transform.position);
                    packet.WriteQuaternion(proxy.Entity.UnityObject.transform.rotation);

                    if (packet.WriteBool(proxy.Entity.IsSceneObject))
                    {
                        NetAssert.False(proxy.Entity.SceneId.IsNone,
                            string.Format("'{0}' is marked a scene object but has no scene id ",
                                proxy.Entity.UnityObject.gameObject));
                        packet.WriteUniqueId(proxy.Entity.SceneId);
                    }
                }

                packCount = proxy.Entity.Serializer.Pack(connection, packet, env);
            }

            if (packet.Overflowing)
            {
                packet.Position = pos;
                return -1;
            }
            if (packCount == -1)
            {
                packet.Position = pos;
                return 0;
            }
            else
            {
                var isForce = proxy.Flags & ProxyFlags.FORCE_SYNC;
                var isCreate = proxy.Flags & ProxyFlags.CREATE_REQUESTED;
                var isDestroy = proxy.Flags & ProxyFlags.DESTROY_REQUESTED;

                // if we didn't pack anything and we are not creating or destroying this, just goto next
                if ((packCount == 0) && !isCreate && !isDestroy && !isForce)
                {
                    packet.Position = pos;
                    return 0;
                }

                // set in progress flags
                if (isDestroy)
                {
                    env.Flags = (proxy.Flags |= ProxyFlags.DESTROY_PENDING);
                }

                // clear force sync flag
                proxy.Flags &= ~ProxyFlags.FORCE_SYNC;

                // clear skipped count
                proxy.Skipped = 0;

                // set packet number
                env.PacketNumber = packet.Number;

                // put on packets list
                packet.EntityUpdates.Enqueue(env);

                // put on proxies pending queue
                // NetLog.Info("adding envelope to {0}, count: {1}", proxy, proxy.Envelopes.Count + 1);
                proxy.Envelopes.Enqueue(env);

                // keep going!
                return 1;
            }
        }

        private bool ReadUpdate(Packet packet)
        {
            if (packet.ReadBool() == false)
            {
                return false;
            }
            // grab networkid
            NetworkId networkId = packet.ReadNetworkId();
            bool isController = packet.ReadBool();
            IMessageRider controlToken = packet.ReadToken();
            bool destroyRequested = packet.ReadBool();
            // we're destroying this proxy
            if (destroyRequested)
            {
                EntityProxy proxy;
                IMessageRider detachToken = packet.ReadToken();

                if (_incommingDict.TryGetValue(networkId, out proxy))
                {
                    if (proxy.Entity.HasControl)
                    {
                        proxy.Entity.ReleaseControlInternal(controlToken);
                    }

                    DestroyIncommingProxy(proxy, detachToken);
                }
                else
                {
                    NetLog.Warn("Received destroy of {0} but no such proxy was found", networkId);
                }
            }
            else
            {
                IMessageRider attachToken = null;

                bool isSceneObject = false;
                bool createRequested = packet.ReadBool();

                UniqueId sceneId = UniqueId.None;
                PrefabId prefabId = new PrefabId();
                TypeId serializerId = new TypeId();
                Vector3 spawnPosition = new Vector3();
                Quaternion spawnRotation = new Quaternion();

                if (createRequested)
                {
                    attachToken = packet.ReadToken();

                    prefabId = packet.ReadPrefabId();
                    serializerId = packet.ReadTypeId();
                    spawnPosition = packet.ReadVector3();
                    spawnRotation = packet.ReadQuaternion();
                    isSceneObject = packet.ReadBool();

                    if (isSceneObject)
                    {
                        sceneId = packet.ReadUniqueId();
                    }
                }

                Entity entity = null;
                EntityProxy proxy = null;

                if (createRequested && (_incommingDict.ContainsKey(networkId) == false))
                {
                    // create entity
                    if (isSceneObject)
                    {
                        GameObject go = Core.FindSceneObject(sceneId);

                        if (!go)
                        {
                            NetLog.Warn("Could not find scene object with {0}", sceneId);
                            go = Core.PrefabPool.Instantiate(prefabId, spawnPosition, spawnRotation);
                        }

                        entity = Entity.CreateFor(go, prefabId, serializerId, EntityFlags.SCENE_OBJECT);
                    }
                    else
                    {
                        GameObject go = Core.PrefabPool.LoadPrefab(prefabId);

                        // prefab checks (if applicable)
                        if (go)
                        {
                            if (Core.IsServer && !go.GetComponent<AscensionEntity>().allowInstantiateOnClient)
                            {
                                throw new AscensionException(
                                    "Received entity of prefab {0} from client at {1}, but this entity is not allowed to be instantiated from clients",
                                    go.name, connection.RemoteEndPoint);
                            }
                        }

                        NetLog.Warn("Creating instance of {0}", prefabId);
                        entity = Entity.CreateFor(prefabId, serializerId, spawnPosition, spawnRotation);
                    }

                    entity.Source = connection;
                    entity.SceneId = sceneId;
                    entity.NetworkId = networkId;

                    // handle case where we are given control (it needs to be true during the initialize, read and attached callbacks)
                    if (isController)
                    {
                        entity.Flags |= EntityFlags.HAS_CONTROL;
                    }

                    // initialize entity
                    entity.Initialize();

                    // create proxy
                    proxy = entity.CreateProxy();
                    proxy.NetworkId = networkId;
                    proxy.Connection = connection;

                    // register proxy
                    _incommingDict.Add(proxy.NetworkId, proxy);

                    // read packet
                    entity.Serializer.Read(connection, packet, packet.Frame);

                    // attach entity
                    proxy.Entity.AttachToken = attachToken;
                    proxy.Entity.Attach();

                    // assign control properly
                    if (isController)
                    {
                        proxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
                        proxy.Entity.TakeControlInternal(controlToken);
                    }

                    // log debug info
                    NetLog.Debug("Received {0} from {1}", entity, connection);

                    // update last received frame
                    proxy.Entity.LastFrameReceived = AscensionNetwork.Frame;
                    proxy.Entity.Freeze(false);

                    // notify user
                    GlobalEventListenerBase.EntityReceivedInvoke(proxy.Entity.UnityObject);
                }
                else
                {
                    // find proxy
                    proxy = _incommingDict[networkId];

                    if (proxy == null)
                    {
                        throw new AscensionException("Couldn't find entity for {0}", networkId);
                    }

                    // update control state yes/no
                    if (proxy.Entity.HasControl ^ isController)
                    {
                        if (isController)
                        {
                            proxy.Entity.TakeControlInternal(controlToken);
                        }
                        else
                        {
                            proxy.Entity.ReleaseControlInternal(controlToken);
                        }
                    }

                    // read update
                    proxy.Entity.Serializer.Read(connection, packet, packet.Frame);
                    proxy.Entity.LastFrameReceived = AscensionNetwork.Frame;
                    proxy.Entity.Freeze(false);
                }
            }

            return true;
        }

        private void DestroyOutgoingProxy(EntityProxy proxy)
        {
            // remove outgoing proxy index
            _outgoingDict.Remove(proxy.NetworkId);

            // remove proxy from entity
            if (proxy.Entity && proxy.Entity.IsAttached)
            {
                proxy.Entity.Proxies.Remove(proxy);
            }

            if (proxy.Flags & ProxyFlags.DESTROY_IGNORE)
            {
                CreateOnRemote(proxy.Entity);
            }
        }

        private void DestroyIncommingProxy(EntityProxy proxy, IMessageRider token)
        {
            // remove incomming proxy
            _incommingDict.Remove(proxy.NetworkId);

            // destroy entity
            proxy.Entity.DetachToken = token;

            // destroy entity
            Core.DestroyForce(proxy.Entity);
        }

    }
}