using System;

namespace Ascension.Networking
{
    /// <summary>
    /// Sets the network mode and scenes that a GlobalEventListener should be run on
    /// </summary>
    /// <example>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class GlobalBehaviorAttribute : Attribute
    {

        /// <summary>
        /// Sets this behaviour to run only in server or client network mode
        /// </summary>
        public NetworkModes Mode
        {
            get;
            private set;
        }

        /// <summary>
        /// A list of scenes for this behaviour to run on
        /// </summary>
        public string[] Scenes
        {
            get;
            private set;
        }

        public GlobalBehaviorAttribute()
          : this(NetworkModes.Host | NetworkModes.Client)
        {
        }

        public GlobalBehaviorAttribute(NetworkModes mode)
          : this(mode, new string[0])
        {
        }

        public GlobalBehaviorAttribute(params string[] scenes)
          : this(NetworkModes.Host | NetworkModes.Client, scenes)
        {
        }

        public GlobalBehaviorAttribute(NetworkModes mode, params string[] scenes)
        {
            this.Mode = mode;
            this.Scenes = scenes;
        }
    }

}
