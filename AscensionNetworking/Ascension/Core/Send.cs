using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    /// <summary>
    /// 
    /// </summary>
    [AscensionExecutionOrder(10000)]
    public class Send : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void FixedUpdate()
        {
            Core.Timer.Stop();
            Core.Timer.Reset();
            Core.Timer.Start();

            Core.Send();

            Core.Timer.Stop();

            DebugInfo.SendTime = (int) Core.Timer.ElapsedMilliseconds;
        }
    }
}
