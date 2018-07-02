using System;
using UnityEngine;

namespace Ascension.Networking
{
    class LoadOp : NetObject
    {
        public SceneLoadState scene;
        public AsyncOperation async;
    }

}