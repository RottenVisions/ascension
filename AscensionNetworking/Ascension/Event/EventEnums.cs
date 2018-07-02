namespace Ascension.Networking
{
    /// <summary>
    /// The reliability mode of an event
    /// </summary>
    public enum ReliabilityModes : byte
    {
        Unreliable = 0,
        //ReliableUnordered = 1,
        ReliableOrdered = 2,
    }

    /// <summary>
    /// The target of an entity event
    /// </summary>
    public enum EntityTargets : byte
    {
        Everyone = Event.ENTITY_EVERYONE,
        EveryoneExceptController = Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER,
        EveryoneExceptOwner = Event.ENTITY_EVERYONE_EXCEPT_OWNER,
        EveryoneExceptOwnerAndController = Event.ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER,
        OnlyController = Event.ENTITY_ONLY_CONTROLLER,
        OnlyControllerAndOwner = Event.ENTITY_ONLY_CONTROLLER_AND_OWNER,
        OnlyOwner = Event.ENTITY_ONLY_OWNER,
        OnlySelf = Event.ENTITY_ONLY_SELF,
    }

    /// <summary>
    /// The target of a global event
    /// </summary>
    public enum GlobalTargets : byte
    {
        Everyone = Event.GLOBAL_EVERYONE,
        Others = Event.GLOBAL_OTHERS,
        AllClients = Event.GLOBAL_ALL_CLIENTS,
        OnlyServer = Event.GLOBAL_ONLY_SERVER,
        OnlySelf = Event.GLOBAL_ONLY_SELF,
    }

}
