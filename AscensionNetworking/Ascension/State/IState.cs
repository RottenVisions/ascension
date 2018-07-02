using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Ascension.Networking
{
    /// <summary>
    /// Base interface for all states
    /// </summary>

    public interface IState : IDisposable
    {

        /// <summary>
        /// The Animator component associated with this entity state
        /// </summary>
        UE.Animator Animator
        {
            get;
        }

        /// <summary>
        /// A collection of all Animator components associated with this entity state
        /// </summary>
        IEnumerable<UE.Animator> AllAnimators
        {
            get;
        }

        /// <summary>
        /// Set the animator object this state should use for reading/writing mecanim parameters
        /// </summary>
        /// <param name="animator">The animator object to use</param>
        void SetAnimator(UE.Animator animator);
        void AddAnimator(UE.Animator animator);

        /// <summary>
        /// Allows you to hook up a callback to a specific property
        /// </summary>
        /// <param name="path">The path of the property</param>
        /// <param name="callback">The callback delegate</param>
        void AddCallback(string path, PropertyCallback callback);

        /// <summary>
        /// Allows you to hook up a callback to a specific property
        /// </summary>
        /// <param name="path">The path of the property</param>
        /// <param name="callback">The callback delegate</param>
        void AddCallback(string path, PropertyCallbackSimple callback);

        /// <summary>
        /// Removes a callback from a property
        /// </summary>
        /// <param name="path">The path of the property</param>
        /// <param name="callback">The callback delegate to remove</param>
        void RemoveCallback(string path, PropertyCallback callback);

        /// <summary>
        /// Removes a callback from a property
        /// </summary>
        /// <param name="path">The path of the property</param>
        /// <param name="callback">The callback delegate to remove</param>
        void RemoveCallback(string path, PropertyCallbackSimple callback);

        /// <summary>
        /// Set a property dynamically by string name
        /// </summary>
        /// <param name="property">The property name to set</param>
        /// <param name="value">The property value to set</param>
        void SetDynamic(string property, object value);

        /// <summary>
        /// Gets a property dynamically by string name
        /// </summary>
        /// <param name="property">The property name to get</param>
        /// <returns></returns>
        object GetDynamic(string property);

        bool TrySetDynamic(string property, object value);
        bool TryGetDynamic(string property, out object value);

        void SetTransforms(NetworkTransform transform, UnityEngine.Transform simulate);
        void SetTransforms(NetworkTransform transform, UnityEngine.Transform simulate, UnityEngine.Transform render);

        void ForceTransform(NetworkTransform transform, UnityEngine.Vector3 position);
        void ForceTransform(NetworkTransform transform, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation);
    }
}
