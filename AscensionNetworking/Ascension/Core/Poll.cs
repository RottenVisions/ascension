using UnityEngine;
using System.Collections;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    /// <summary>
    /// Ascension behaviour to poll the network and step entities in the simulation
    /// </summary>
    [AscensionExecutionOrder(-10000)]
    public class Poll : MonoBehaviour
    {
        [HideInInspector]
        public bool AllowImmediateShutdown = true;

        void Awake()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            try
            {
                if ((Time.timeScale != 1f) && RuntimeSettings.Instance.overrideTimeScale)
                {
                    // log this error
                    NetLog.Error("Time.timeScale value is incorrect: {0}f", Time.timeScale);

                    // force this
                    Time.timeScale = 1f;

                    // log that we forced timescale to 1
                    NetLog.Error("Time.timeScale has been set to 1.0f by Ascension");
                }
            }
            finally
            {
                Core.Update();
            }
        }

        void FixedUpdate()
        {
            Core.Timer.Stop();
            Core.Timer.Reset();
            Core.Timer.Start();

            Core.Poll();

            Core.Timer.Stop();

            DebugInfo.PollTime = (int)Core.Timer.ElapsedMilliseconds;
        }

        void OnDisable()
        {
            if (Application.isEditor && AllowImmediateShutdown)
            {
                Core.ShutdownImmediate();
            }
        }

        void OnDestroy()
        {
            if (Application.isEditor && AllowImmediateShutdown)
            {
                Core.ShutdownImmediate();
            }
        }

        void OnApplicationQuit()
        {
            if (AllowImmediateShutdown)
            {
                //Application.CancelQuit();
                //StartCoroutine(DelayedQuit());
                Core.ShutdownImmediate();
            }
        }

        IEnumerator DelayedQuit()
        {
            Core.ShutdownImmediate();
            for (int i = 0; i < Core.FramesPerSecond; i++)
            {
                yield return null;
            }
            Application.Quit();
        }
    }
}