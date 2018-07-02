using UnityEngine;
using System.Collections;
using Ascender;

namespace Ascension.Networking.Sockets
{
    public class SocketEventListenerBase : MonoBehaviour, IListNode
    {
        static readonly ListExtended<SocketEventListenerBase> Callbacks = new ListExtended<SocketEventListenerBase>();

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


        public virtual void OnMessageReceieved(NetPeer peer, SocketConnection c, int dataSize, byte[] data, string msgName) { }

        public static void OnMessageReceievedInvoke(NetPeer peer, SocketConnection c, int dataSize, byte[] data, string msgName)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback OnMessageReceieved");
#endif
            foreach (SocketEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.OnMessageReceieved(peer, c, dataSize, data, msgName);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }
    }
}
