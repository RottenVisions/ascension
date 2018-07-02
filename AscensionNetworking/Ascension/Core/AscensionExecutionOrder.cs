using System;

namespace Ascension.Networking
{
    /// <summary>
    /// Sets the Unity script execution order
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AscensionExecutionOrderAttribute : Attribute
    {
        readonly int executionOrder;

        public AscensionExecutionOrderAttribute(int order)
        {
            executionOrder = order;
        }

        /// <summary>
        /// The order of this script in execution (lower is earlier)
        /// </summary>
        public int ExecutionOrder
        {
            get { return executionOrder; }
        }
    }

}
