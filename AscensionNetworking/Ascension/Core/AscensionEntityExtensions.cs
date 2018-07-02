using System;
using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    public static class AscensionEntityExtensions
    {
        public static bool IsAttached(this AscensionEntity entity)
        {
            return AscensionNetwork.IsRunning && entity && entity.IsAttached;
        }

        public static bool IsOwner(this AscensionEntity entity)
        {
            return entity.IsAttached() && entity.IsOwner;
        }

        public static bool IsControlled(this AscensionEntity entity)
        {
            return entity.IsAttached() && entity.IsControlled;
        }

        public static bool IsSceneObject(this AscensionEntity entity)
        {
            return entity.IsAttached() && entity.IsSceneObject;
        }

        public static bool IsFrozen(this AscensionEntity entity)
        {
            return entity.IsAttached() && entity.IsFrozen;
        }

        public static bool HasControl(this AscensionEntity entity)
        {
            return entity.IsAttached() && entity.HasControl;
        }
    }
}