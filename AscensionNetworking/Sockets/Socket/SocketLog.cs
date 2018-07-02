using System;
using System.Diagnostics;
using System.Threading;

namespace Ascension.Networking.Sockets
{
    public static class SocketLog
    {
        public delegate void Writer(uint level, string message);

        public const uint ERROR = 0;
        public const uint INFO = 1;
        public const uint DEBUG = 4;
        public const uint TRACE = 8;
        public const uint WARN = 16;

        static uint enabled = INFO | DEBUG | TRACE | WARN | ERROR;
        static Writer writer = null;
        static readonly object sync = new object();

        static void Write(uint level, string message)
        {
            lock (sync)
            {
                Writer callback = writer;

                if (callback != null)
                {
                    callback(level, DateTime.Now + ": " + message);
                }
            }
        }

        static public void Info(string format, params object[] args)
        {
            if (NetMath.IsSet(enabled, INFO))
                Write(INFO, String.Format(format, args));
        }

        [Conditional("TRACE")]
        static public void Trace(string format, params object[] args)
        {
#if TRACE
            if (NetMath.IsSet(enabled, TRACE))
                Write(TRACE, String.Format(format, args));
#endif
        }

        [Conditional("DEBUG")]
        static public void Debug(string format, params object[] args)
        {
#if DEBUG
            if (NetMath.IsSet(enabled, DEBUG))
                Write(DEBUG, String.Format(format, args));
#endif
        }

        static public void Warn(string format, params object[] args)
        {
            if (NetMath.IsSet(enabled, WARN))
            {
                Write(WARN, String.Format(format, args));
            }
        }

        static public void Error(string format, params object[] args)
        {
            Write(ERROR, String.Format(format, args));
        }

        static public void SetWriter(Writer callback)
        {
            writer = callback;
        }

        static public void Disable(uint flag)
        {
            enabled &= ~flag;
        }

        static public void Enable(uint flag)
        {
            enabled |= flag;
        }

        static public bool IsEnabled(uint flag)
        {
            return (enabled & flag) == flag;
        }
    }
}