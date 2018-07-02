using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{

    public abstract partial class BaseGlobalEventListener : MonoBehaviour, IListNode
    {
        private static readonly ListExtended<BaseGlobalEventListener> Callbacks =
            new ListExtended<BaseGlobalEventListener>();

        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        protected void OnEnable()
        {
            Core.GlobalEventDispatcher.Add(this);
            Callbacks.AddLast(this);
        }

        protected void OnDisable()
        {
            Core.GlobalEventDispatcher.Remove(this);
            Callbacks.Remove(this);
        }

        /// <summary>
        /// Override this method and return true if you want the event listener to keep being attached to Ascension even when Ascension shuts down and starts again.
        /// </summary>
        /// <returns>True/False</returns>
        /// <example>
        /// *Example:* Configuring the persistence behaviour to keep this listener alive between startup and shutdown.
        /// 
        /// ```csharp
        /// public override bool PersistBetweenStartupAndShutdown() {
        ///   return true;
        /// }
        /// ```
        /// </example>
        public virtual bool PersistBetweenStartupAndShutdown()
        {
            return false;
        }
    }
}
