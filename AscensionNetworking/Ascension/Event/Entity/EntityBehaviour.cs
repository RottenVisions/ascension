using System;
using UnityEngine;

namespace Ascension.Networking
{
    /// <summary>
    /// Base class for unity behaviours that want to access Ascension methods
    /// </summary>
    public abstract class EntityBehaviour : MonoBehaviour, IEntityBehaviour
    {
        [HideInInspector]
        public AscensionEntity _entity;

        /// <summary>
        /// The entity for this behaviour
        /// </summary>
        /// Use the ```entity``` property to access the public ```AscensionEntity``` of the gameObject that this script is attached to.
        public AscensionEntity entity
        {
            get
            {
                if (!_entity)
                {
                    Transform t = transform;

                    while (t && !_entity)
                    {
                        _entity = t.GetComponent<AscensionEntity>();
                        t = t.parent;
                    }

                    if (!_entity)
                    {
                        NetLog.Error("Could not find a Ascension Entity component attached to '{0}' or any of its parents", gameObject.name);
                    }
                }

                return _entity;
            }
            set
            {
                _entity = value;
            }
        }

        Boolean IEntityBehaviour.Invoke
        {
            get
            {
                return enabled;
            }
        }

        /// <summary>
        /// Invoked when the entity has been initialized, before Attached
        /// </summary>
        public virtual void Initialized() { }

        /// <summary>
        /// Invoked when Ascension is aware of this entity and all public state has been setup
        /// </summary>
        public virtual void Attached() { }

        /// <summary>
        /// Invoked when this entity is removed from Ascension's awareness
        /// </summary>
        public virtual void Detached() { }

        /// <summary>
        /// Invoked each simulation step on the owner
        /// </summary>
        public virtual void SimulateOwner() { }

        /// <summary>
        /// Invoked each simulation step on the controller
        /// </summary>
        public virtual void SimulateController() { }

        /// <summary>
        /// Invoked when you gain control of this entity
        /// </summary>
        public virtual void ControlGained() { }

        /// <summary>
        /// Invoked when you gain control of this entity
        /// </summary>
        /// <param name="token">A data token of max size 512 bytes</param> 
        public virtual void ControlGained(IMessageRider token) { }

        /// <summary>
        /// Invoked when you lost control of this entity
        /// </summary>
        public virtual void ControlLost() { }

        /// <summary>
        /// Invoked when you lost control of this entity
        /// </summary>
        /// <param name="token">A data token of max size 512 bytes</param>
        public virtual void ControlLost(IMessageRider token) { }

        /// <summary>
        /// Invoked on the owner when a remote connection is controlling this entity but we have not received any command for the current simulation frame.
        /// </summary>
        /// <param name="previous">The last valid command received</param>
        public virtual void MissingCommand(Command previous) { /*Debug.Log("Missing command!");*/}

        /// <summary>
        /// Invoked on both the owner and controller to execute a command
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="resetState">Indicates if we should reset the state of the local motor or not</param>
        public virtual void ExecuteCommand(Command command, bool resetState) { }


    }

    /// <summary>
    /// Base class for unity behaviours that want to access Ascension methods with the state available also
    /// </summary>
    /// <typeparam name="TState">The type of state on this AscensionEntity</typeparam>
    public abstract class EntityBehaviour<TState> : EntityBehaviour
    {

        /// <summary>
        /// The state for this behaviours entity
        /// </summary>
        public TState state
        {
            get { return entity.GetState<TState>(); }
        }
    }
}

namespace Ascension.Networking
{
    public abstract class EntityEventListenerBase : EntityBehaviour
    {
        public sealed override void Initialized()
        {
            entity.AEntity.AddEventListener(this);
        }
    }

    public abstract class EntityEventListenerBase<TState> : EntityEventListenerBase
    {
        public TState state
        {
            get { return entity.GetState<TState>(); }
        }
    }
}
