using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public interface IEntitySerializer
    {
        TypeId TypeId { get; }

        void OnRender();
        void OnInitialized();
        void OnCreated(Entity entity);
        void OnParentChanging(Entity newParent, Entity oldParent);

        void OnSimulateBefore();
        void OnSimulateAfter();

        void OnControlGained();
        void OnControlLost();

        BitSet GetDefaultMask();
        BitSet GetFilter(Connection connection, EntityProxy proxy);

        void DebugInfo();
        void InitProxy(EntityProxy p);

        int Pack(Connection connection, Packet stream, EntityProxyEnvelope proxy);
        void Read(Connection connection, Packet stream, int frame);
    }

    public interface IEntitySerializer<TState> : IEntitySerializer where TState : IState
    {
        TState state { get; }
    }

}