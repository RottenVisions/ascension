using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    /// <summary>
    /// Interface which can be implemented on a behaviour attached to an entity which lets you provide
    /// custom priority calculations for state and events.
    /// </summary>
    public interface IPriorityCalculator
    {
        /// <summary>
        /// Called for calculating the priority of this entity for the connection passed in
        /// </summary>
        /// <param name="connection">The connection we are calculating priority for</param>
        /// <param name="mask">The mask of properties with updated values we want to replicate</param>
        /// <param name="skipped">How many packets since we sent an update for this entity</param>
        /// <returns>The priority of the entity</returns>
        float CalculateStatePriority(Connection connection, int skipped);

        /// <summary>
        /// Called for calculating the priority of an event sent to this entity for the connection passed in
        /// </summary>
        /// <param name="connection">The connection we are calculating priority for</param>
        /// <param name="evnt">The event we are calculating priority for</param>
        /// <returns>The priority of the event</returns>
        float CalculateEventPriority(Connection connection, Event evnt);

        bool Always { get; }
    }

}