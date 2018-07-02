using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Ascension.Networking
{
    public class SceneLoader : MonoBehaviour
    {
        static int delay;
        static SceneLoadState loaded;

        static readonly ListExtendedSingular<LoadOp> LoadOps = new ListExtendedSingular<LoadOp>();
        static public bool IsLoading { get { return LoadOps.Count > 0; } }

        void Update()
        {
            if (LoadOps.Count > 0)
            {
                LoadAsync();
            }
            else
            {
                if (delay > 0)
                {
                    if (--delay == 0)
                    {
                        if (LoadOps.Count == 0)
                        {
                            Core.SceneLoadDone(loaded);
                        }
                    }
                }
            }
        }

        void Load()
        {
            // notify core of loading
            Core.SceneLoadBegin(LoadOps.First.scene);

            // load level
            SceneManager.LoadSceneAsync(AscensionNetworkInternal.GetSceneName(LoadOps.First.scene.Scene.Index));

            // we are done!
            Done();
        }

        void LoadAsync()
        {
            if (LoadOps.First.async == null)
            {
                // notify core of loading
                Core.SceneLoadBegin(LoadOps.First.scene);

                // begin new async load
                LoadOps.First.async = SceneManager.LoadSceneAsync(AscensionNetworkInternal.GetSceneName(LoadOps.First.scene.Scene.Index));

            }
            else
            {
                if (LoadOps.First.async.isDone)
                {
                    Done();
                }
            }
        }

        void Done()
        {
            try
            {
                GC.Collect();

                loaded = LoadOps.RemoveFirst().scene;
            }
            finally
            {
                if (LoadOps.Count == 0)
                {
                    delay = 60;
                }
            }
        }

        public static void Enqueue(SceneLoadState scene)
        {
            NetLog.Debug("Loading {0} ({1})", scene, AscensionNetworkInternal.GetSceneName(scene.Scene.Index));

            delay = 0;
            LoadOps.AddLast(new LoadOp { scene = scene });
        }
    }

}
