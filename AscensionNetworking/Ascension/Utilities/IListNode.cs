namespace Ascension.Networking
{
    public interface IListNode
    {
        object Prev { get; set; }
        object Next { get; set; }
        object List { get; set; }
    }
}
