#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using IO = System.IO;
using SYS = System;
using UE = UnityEngine;

namespace Ascension.Networking
{
    /// <summary>
    /// Provides logging capabilities to a variety of outputs
    /// </summary>
    public static class NetLog
    {
        /// <summary>
        /// The interface providing log writing capabilities to an output
        /// </summary>
        public interface IWriter : IDisposable
        {
            void Info(string message);
            void Debug(string message);
            void Warn(string message);
            void Error(string message);
        }

        /// <summary>
        /// IWriter implementation that outputs to a file
        /// </summary>
        public class File : IWriter
        {
            volatile bool running = true;

            bool isServer;
            Thread thread;
            AutoResetEvent threadEvent;
            Queue<string> threadQueue;

            public File(bool server)
            {
                isServer = server;
                threadEvent = new AutoResetEvent(false);
                threadQueue = new Queue<string>(1024);

                thread = new Thread(WriteLoop);
                thread.IsBackground = true;
                thread.Start();
            }

            void Queue(string message)
            {
                lock (threadQueue)
                {
                    threadQueue.Enqueue(message);
                    threadEvent.Set();
                }
            }

            void IWriter.Info(string message)
            {
                Queue(message);
            }

            void IWriter.Debug(string message)
            {
                Queue(message);
            }

            void IWriter.Warn(string message)
            {
                Queue(message);
            }

            void IWriter.Error(string message)
            {
                Queue(message);
            }

            public void Dispose()
            {
                running = false;
            }

            void WriteLoop()
            {
                try
                {
                    var n = DateTime.Now;

                    string logFile;
                    logFile = "Ascension_Log_{7}_{0}Y-{1}M-{2}D_{3}H{4}M{5}S_{6}MS.txt";
                    logFile = string.Format(logFile, n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second, n.Millisecond, isServer ? "SERVER" : "CLIENT");

                    var stream = IO.File.Open(logFile, IO.FileMode.Create);
                    var streamWriter = new IO.StreamWriter(stream);

                    while (running)
                    {
                        if (threadEvent.WaitOne(100))
                        {
                            lock (threadQueue)
                            {
                                while (threadQueue.Count > 0)
                                {
                                    streamWriter.WriteLine(threadQueue.Dequeue());
                                }
                            }
                        }

                        streamWriter.Flush();
                        stream.Flush();
                    }

                    streamWriter.Flush();
                    streamWriter.Close();
                    streamWriter.Dispose();

                    stream.Flush();
                    stream.Close();
                    stream.Dispose();

                    threadEvent.Close();
                }
                catch (Exception exn)
                {
                    Exception(exn);
                }
            }
        }

        /// <summary>
        /// IWriter implementation that outputs to the Ascension console
        /// </summary>
        public class NetConsoleOut : IWriter
        {
            void IWriter.Info(string message)
            {
                NetConsole.Write(message, NetGUI.Sky);
            }

            void IWriter.Debug(string message)
            {
                NetConsole.Write(message, NetGUI.Green);
            }

            void IWriter.Warn(string message)
            {
                NetConsole.Write(message, NetGUI.Orange);
            }

            void IWriter.Error(string message)
            {
                NetConsole.Write(message, NetGUI.Error);
            }

            public void Dispose()
            {

            }
        }

        /// <summary>
        /// IWriter implementation that outputs to the system console out
        /// </summary>
        public class SystemOut : IWriter
        {
            void IWriter.Info(string message)
            {
                SYS.Console.Out.WriteLine(message);
            }

            void IWriter.Debug(string message)
            {
                SYS.Console.Out.WriteLine(message);
            }

            void IWriter.Warn(string message)
            {
                SYS.Console.Out.WriteLine(message);
            }

            void IWriter.Error(string message)
            {
                SYS.Console.Error.WriteLine(message);
            }

            public void Dispose()
            {

            }
        }

        /// <summary>
        /// IWriter implementation that outputs to Unity console
        /// </summary>
        public class Unity : IWriter
        {
            void IWriter.Info(string message)
            {
                UE.Debug.Log(message);
            }

            void IWriter.Debug(string message)
            {
                UE.Debug.Log(message);
            }

            void IWriter.Warn(string message)
            {
                UE.Debug.LogWarning(message);
            }

            void IWriter.Error(string message)
            {
                UE.Debug.LogError(message);
            }

            public void Dispose()
            {

            }
        }

        static readonly object _lock = new object();
        static List<IWriter> _writers = new List<IWriter>();

        public static void RemoveAll()
        {
            lock (_lock)
            {
                for (int i = 0; i < _writers.Count; ++i)
                {
                    _writers[i].Dispose();
                }

                _writers = new List<IWriter>();
            }
        }

        public static void Add<T>(T instance) where T : class, IWriter
        {
            lock (_lock)
            {
                _writers.Add(instance);
            }
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(string message)
        {
            lock (_lock)
            {
                VerifyOneWriter();

                for (int i = 0; i < _writers.Count; ++i)
                {
                    _writers[i].Info(message);
                }
            }
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(object message)
        {
            Info(Format(message));
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(string message, object arg0)
        {
            Info(Format(message, arg0));
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(string message, object arg0, object arg1)
        {
            Info(Format(message, arg0, arg1));
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(string message, object arg0, object arg1, object arg2)
        {
            Info(Format(message, arg0, arg1, arg2));
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Info(string message, params object[] args)
        {
            Info(Format(message, args));
        }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
        public static void Debug(string message)
        {
            lock (_lock)
            {
                VerifyOneWriter();

                for (int i = 0; i < _writers.Count; ++i)
                {
                    _writers[i].Debug(message);
                }
            }
        }

[Conditional("DEBUG")]
        public static void Debug(object message)
        {
            Debug(Format(message));
        }

[Conditional("DEBUG")]
        public static void Debug(string message, object arg0)
        {
            Debug(Format(message, arg0));
        }

[Conditional("DEBUG")]
        public static void Debug(string message, object arg0, object arg1)
        {
            Debug(Format(message, arg0, arg1));
        }

[Conditional("DEBUG")]
        public static void Debug(string message, object arg0, object arg1, object arg2)
        {
            Debug(Format(message, arg0, arg1, arg2));
        }

[Conditional("DEBUG")]
        public static void Debug(string message, params object[] args)
        {
            Debug(Format(message, args));
        }

        static void VerifyOneWriter()
        {
            if (_writers.Count == 0)
            {
                //_writers.Add(new Unity());
            }
        }

[Conditional("DEBUG")]
        public static void Warn(string message)
        {
            lock (_lock)
            {
                VerifyOneWriter();

                for (int i = 0; i < _writers.Count; ++i)
                {
                    _writers[i].Warn(message);
                }
            }
        }

[Conditional("DEBUG")]
        public static void Warn(object message)
        {
            Warn(Format(message));
        }

[Conditional("DEBUG")]
        public static void Warn(string message, object arg0)
        {
            Warn(Format(message, arg0));
        }

[Conditional("DEBUG")]
        public static void Warn(string message, object arg0, object arg1)
        {
            Warn(Format(message, arg0, arg1));
        }

[Conditional("DEBUG")]
        public static void Warn(string message, object arg0, object arg1, object arg2)
        {
            Warn(Format(message, arg0, arg1, arg2));
        }

[Conditional("DEBUG")]
        public static void Warn(string message, params object[] args)
        {
            Warn(Format(message, FixNulls(args)));
        }

        static object[] FixNulls(object[] args)
        {
            if (args == null)
            {
                args = new object[0];
            }

            for (int i = 0; i < args.Length; ++i)
            {
                if (ReferenceEquals(args[i], null))
                {
                    args[i] = "NULL";
                }
            }

            return args;
        }

[Conditional("DEBUG")]
        public static void Error(string message)
        {
            lock (_lock)
            {
                VerifyOneWriter();

                for (int i = 0; i < _writers.Count; ++i)
                {
                    _writers[i].Error(message);
                }
            }
        }

[Conditional("DEBUG")]
        public static void Error(object message)
        {
            Error(Format(message));
        }

[Conditional("DEBUG")]
        public static void Error(string message, object arg0)
        {
            Error(Format(message, arg0));
        }

[Conditional("DEBUG")]
        public static void Error(string message, object arg0, object arg1)
        {
            Error(Format(message, arg0, arg1));
        }

[Conditional("DEBUG")]
        public static void Error(string message, object arg0, object arg1, object arg2)
        {
            Error(Format(message, arg0, arg1, arg2));
        }

[Conditional("DEBUG")]
        public static void Error(string message, params object[] args)
        {
            Error(Format(message, args));
        }

        public static void Exception(Exception exception)
        {
            lock (_lock)
            {
                UnityEngine.Debug.LogException(exception);

                for (int i = 0; i < _writers.Count; ++i)
                {
                    if (!(_writers[i] is Unity))
                    {
                        _writers[i].Error(exception.GetType() + ": " + exception.Message);
                        _writers[i].Error(exception.StackTrace);
                    }
                }
            }
        }


        static string Format(object message)
        {
            return message == null ? "NULL" : message.ToString();
        }

        static string Format(string message, object arg0)
        {
            return string.Format(Format(message), Format(arg0));
        }

        static string Format(string message, object arg0, object arg1)
        {
            return string.Format(Format(message), Format(arg0), Format(arg1));
        }

        static string Format(string message, object arg0, object arg1, object arg2)
        {
            return string.Format(Format(message), Format(arg0), Format(arg1), Format(arg2));
        }

        static string Format(string message, object[] args)
        {
            if (args == null)
            {
                return Format(message);
            }

            for (int i = 0; i < args.Length; ++i)
            {
                args[i] = Format(args[i]);
            }

            return string.Format(Format(message), args);
        }

        public static void Setup(NetworkModes mode, ConfigLogTargets logTargets)
        {
#if DEBUG
            // init loggers
            var fileLog = (logTargets & ConfigLogTargets.File) == ConfigLogTargets.File;
            var unityLog = (logTargets & ConfigLogTargets.Unity) == ConfigLogTargets.Unity;
            var consoleLog = (logTargets & ConfigLogTargets.Console) == ConfigLogTargets.Console;
            var systemOutLog = (logTargets & ConfigLogTargets.SystemOut) == ConfigLogTargets.SystemOut;

            if (unityLog) { Add(new Unity()); }
            if (consoleLog) { Add(new NetConsoleOut()); }
            if (systemOutLog) { Add(new SystemOut()); }
            if (fileLog)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.OSXPlayer:
                        Add(new File(mode == NetworkModes.Host));
                        break;
                }
            }
#endif
        }
    }
}
