namespace Ascension.Networking
{
    /// <summary>
    /// Interface that can be implemented to create custom event filtering rules
    /// </summary>
    public interface IEventFilter
    {
        /// <summary>
        /// Called when a new event is recieved
        /// </summary>
        /// <param name="ev">The event data</param>
        /// <returns>Whether to accept or reject the event</returns>
        bool EventReceived(Event ev);
    }

    /// <summary>
    /// Default implementation of IEventFilter that lets everything through
    /// </summary>
    public class DefaultEventFilter : IEventFilter
    {
        bool IEventFilter.EventReceived(Event ev)
        {
            return true;
        }
    }
}
