using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public enum SortOrder
    {
        Manual,
        Name,
        Priority
    }

    [ProtoContract]
    [ProtoInclude(100, typeof (StateDefinition))]
    [ProtoInclude(200, typeof (EventDefinition))]
    [ProtoInclude(300, typeof (ObjectDefinition))]
    [ProtoInclude(400, typeof (CommandDefinition))]
    public abstract class AssetDefinition
    {
        [ProtoMember(5)] public string Comment;
        [ProtoIgnore] public bool Deleted;
        [ProtoMember(6)] public bool Enabled;
        [ProtoMember(9, OverwriteList = true)] public HashSet<string> Groups = new HashSet<string>();
        [ProtoMember(1)] public Guid Guid;
        [ProtoMember(2)] public string Name;
        [ProtoIgnore] public Project Project;
        [ProtoMember(10)] public SortOrder SortOrder;
        public abstract IEnumerable<Type> AllowedPropertyTypes { get; }
    }
}