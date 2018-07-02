using System;

namespace Ascension.Networking
{
    public interface IFactory
    {
        Type TypeObject { get; }
        TypeId TypeId { get; }
        UniqueId TypeKey { get; }
        object Create();
    }

    public interface IEventFactory : IFactory
    {
        void Dispatch(Event ev, object target);
    }

    public interface ISerializerFactory : IFactory
    {
    }

    public interface ICommandFactory : IFactory
    {
    }
}
