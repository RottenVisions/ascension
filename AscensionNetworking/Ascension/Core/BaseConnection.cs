using System;
using UnityEngine;

namespace Ascension.Networking
{
    public class BaseConnection : NetObject, IDisposable
    {
        protected bool connected;
        protected bool rejected;
        protected string ipAddress;
        protected string operatingSystem;
        protected float connectionTime;

        private bool disposed;

        public int SecondsConnected
        {
            get { return (int) (Time.realtimeSinceStartup - connectionTime); }
        }

        public BaseConnection()
        {

        }

        public virtual void Initialize()
        {

        }

        public virtual void Initialize(string address, int hostId, int connectionId)
        {

        }

        ~BaseConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {

            }

            disposed = true;
        }
    }
}
