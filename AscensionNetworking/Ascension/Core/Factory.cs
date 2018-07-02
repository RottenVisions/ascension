using System;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public static class Factory
    {
        static Dictionary<byte, Type> _id2token = new Dictionary<byte, Type>();
        static Dictionary<Type, byte> _token2id = new Dictionary<Type, byte>();

        static Dictionary<Type, IFactory> _factoriesByType = new Dictionary<Type, IFactory>();
        static Dictionary<TypeId, IFactory> _factoriesById = new Dictionary<TypeId, IFactory>();
        static Dictionary<UniqueId, IFactory> _factoriesByKey = new Dictionary<UniqueId, IFactory>();

        public static bool IsEmpty
        {
            get { return _factoriesById.Count == 0; }
        }

        public static void Register(IFactory factory)
        {
            _factoriesById.Add(factory.TypeId, factory);
            _factoriesByKey.Add(factory.TypeKey, factory);
            _factoriesByType.Add(factory.TypeObject, factory);
        }

        public static IFactory GetFactory(TypeId id)
        {
#if DEBUG
            if (!_factoriesById.ContainsKey(id))
            {
                NetLog.Error("Unknown factory {0}", id);
                return null;
            }
#endif

            return _factoriesById[id];
        }

        public static IFactory GetFactory(UniqueId id)
        {
#if DEBUG
            if (!_factoriesByKey.ContainsKey(id))
            {
                NetLog.Error("Unknown factory {0}", id);
                return null;
            }
#endif

            return _factoriesByKey[id];
        }

        public static IEventFactory GetEventFactory(TypeId id)
        {
            return (IEventFactory)_factoriesById[id];
        }

        public static IEventFactory GetEventFactory(UniqueId id)
        {
            return (IEventFactory)_factoriesByKey[id];
        }

        public static Event NewEvent(TypeId id)
        {
            Event ev;

            ev = (Event)Create(id);
            ev.IncrementRefs();

            return ev;
        }

        public static Event NewEvent(UniqueId id)
        {
            Event ev;

            ev = (Event)Create(id);
            ev.IncrementRefs();

            return ev;
        }

        public static byte GetTokenId(IMessageRider obj)
        {
#if DEBUG
            if (_token2id.ContainsKey(obj.GetType()) == false)
            {
                throw new AscensionException("Unknown token type {0}", obj.GetType());
            }
#endif

            return _token2id[obj.GetType()];
        }

        public static IMessageRider NewToken(byte id)
        {
#if DEBUG
            if (_id2token.ContainsKey(id) == false)
            {
                throw new AscensionException("Unknown token id {0}", id);
            }
#endif

            return (IMessageRider)Activator.CreateInstance(_id2token[id]);
        }

        public static Command NewCommand(TypeId id)
        {
            return (Command)Create(id);
        }

        public static Command NewCommand(UniqueId id)
        {
            return (Command)Create(id);
        }

        public static IEntitySerializer NewSerializer(TypeId id)
        {
            return (IEntitySerializer)Create(id);
        }

        public static IEntitySerializer NewSerializer(UniqueId guid)
        {
            return (IEntitySerializer)Create(guid);
        }

        static object Create(TypeId id)
        {
#if DEBUG
            if (_factoriesById.ContainsKey(id) == false)
            {
                NetLog.Error("Unknown {0}", id);
            }
#endif

            return _factoriesById[id].Create();
        }

        static object Create(UniqueId id)
        {
#if DEBUG
            if (_factoriesByKey.ContainsKey(id) == false)
            {
                NetLog.Error("Unknown {0}", id);
            }
#endif

            return _factoriesByKey[id].Create();
        }

        public static void UnregisterAll()
        {
            _token2id.Clear();
            _id2token.Clear();

            _factoriesById.Clear();
            _factoriesByKey.Clear();
            _factoriesByType.Clear();

            _token2id = new Dictionary<Type, byte>();
            _id2token = new Dictionary<byte, Type>();

            _factoriesById = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
            _factoriesByKey = new Dictionary<UniqueId, IFactory>(128, UniqueId.EqualityComparer.Instance);
            _factoriesByType = new Dictionary<Type, IFactory>();
        }

        public static void RegisterTokenClass(Type type)
        {
            if (_token2id.Count == 255)
            {
                throw new ArgumentException("Can only register 255 different token types");
            }

            byte id = (byte)(_token2id.Count + 1);

            _token2id.Add(type, id);
            _id2token.Add(id, type);

            NetLog.Debug("Registered token class {0} as id {1}", type, id);
        }
    }
}
