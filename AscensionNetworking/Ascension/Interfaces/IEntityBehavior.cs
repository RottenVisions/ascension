namespace Ascension.Networking
{
    /// <summary>
    /// Interface for unity behaviours that want to access Ascension methods
    /// </summary>
    public interface IEntityBehaviour
    {
        bool Invoke { get; }
        AscensionEntity entity { get; set; }
        void Initialized();

        void Attached();
        void Detached();

        void SimulateOwner();
        void SimulateController();

        void ControlLost();
        void ControlGained();

        void MissingCommand(Command previous);
        void ExecuteCommand(Command command, bool resetState);
    }

    /// <summary>
    /// Interface for unity behaviours that want to access Ascension methods
    /// </summary>
    /// <typeparam name="TState">Ascension state of the entity</typeparam>
    public interface IEntityBehaviour<TState> : IEntityBehaviour where TState : IState
    {
        TState state { get; }
    }

    public interface IEntityReplicationFilter
    {
        bool AllowReplicationTo(Connection connection);
    }
}
