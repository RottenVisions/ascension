using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class Project
    {
        [ProtoMember(4)] public string ActiveGroup;
        [ProtoMember(2)] public FilterDefinition[] Filters = new FilterDefinition[0];
        [ProtoMember(3)] public bool Merged;
        [ProtoMember(1)] public AssetFolder RootFolder = new AssetFolder();

        public IEnumerable<string> Groups
        {
            get { return RootFolder.Assets.SelectMany(x => x.Groups).Distinct(); }
        }

        public IEnumerable<FilterDefinition> EnabledFilters
        {
            get { return Filters.Where(x => x.Enabled); }
        }

        public IEnumerable<StateDefinition> States
        {
            get { return RootFolder.Assets.Where(x => x is StateDefinition).Cast<StateDefinition>(); }
        }

        public IEnumerable<ObjectDefinition> Structs
        {
            get { return RootFolder.Assets.Where(x => x is ObjectDefinition).Cast<ObjectDefinition>(); }
        }

        public IEnumerable<EventDefinition> Events
        {
            get { return RootFolder.Assets.Where(x => x is EventDefinition).Cast<EventDefinition>(); }
        }

        public IEnumerable<CommandDefinition> Commands
        {
            get { return RootFolder.Assets.Where(x => x is CommandDefinition).Cast<CommandDefinition>(); }
        }

        public bool UseFilters
        {
            get { return false; }
        }

        public StateDefinition FindState(Guid guid)
        {
            return States.First(x => x.Guid == guid);
        }

        public void GenerateCode(string file)
        {
            CodeGenerator cg;

            cg = new CodeGenerator();
            cg.Run(this, file);
        }

        public IEnumerable<Guid> GetInheritanceTree(StateDefinition def)
        {
            List<Guid> result = new List<Guid>();
            GetInheritanceTree(def, result);
            return result;
        }

        public void GetInheritanceTree(StateDefinition def, List<Guid> result)
        {
            result.Add(def.Guid);

            foreach (StateDefinition state in States.Where(x => x.ParentGuid == def.Guid))
            {
                GetInheritanceTree(state, result);
            }
        }
    }
}