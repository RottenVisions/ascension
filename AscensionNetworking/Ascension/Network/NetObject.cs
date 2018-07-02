namespace Ascension.Networking
{
    /// <summary>
    /// Utility base class for some common functionality inside of Ascension
    /// </summary>
    public class NetObject : IListNode
    {
#if DEBUG
        public bool pooled = true;
#endif

        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        public static implicit operator bool(NetObject obj)
        {
            return obj != null;
        }
    }
}
