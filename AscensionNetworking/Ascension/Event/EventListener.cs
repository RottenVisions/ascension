namespace Ascension.Networking
{

    /// <summary>
    /// Interface that can be implemented on GlobalEventListener, EntityEventListener and EntityEventListener<T> 
    /// to modify its invoke condition settings
    /// </summary>
    public interface IEventListener
    {
        bool InvokeIfDisabled { get; }
        bool InvokeIfGameObjectIsInactive { get; }
    }
}