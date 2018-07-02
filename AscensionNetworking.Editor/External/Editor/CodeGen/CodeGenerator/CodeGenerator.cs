using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ascension.Networking.Sockets;

namespace Ascension.Compiler
{
    public class CodeGenerator
    {
        public CodeCompileUnit CodeCompileUnit;
        public CodeNamespace CodeNamespace;
        public CodeNamespace CodeNamespaceAscension;
        public CodeNamespace CodeNamespaceAscensionInternal;
        public List<CommandDecorator> Commands;
        public List<EventDecorator> Events;
        public List<ObjectDecorator> Objects;
        private Project Project;
        public List<StateDecorator> States;

        public CodeGenerator()
        {
            States = new List<StateDecorator>();
            Objects = new List<ObjectDecorator>();
            Events = new List<EventDecorator>();
            Commands = new List<CommandDecorator>();
        }

        public IEnumerable<FilterDefinition> Filters
        {
            get { return Project.EnabledFilters; }
        }

        public void Run(Project context, string file)
        {
            Project = context;

            if(RuntimeSettings.Instance.useNetworkDataNamespace)
                CodeNamespace = new CodeNamespace("Ascension.Networking.Data");
            else
                CodeNamespace = new CodeNamespace();

            CodeNamespaceAscension = new CodeNamespace("Ascension.Networking");
            CodeNamespaceAscensionInternal = new CodeNamespace("Ascension.Networking");
            //Add 'using' statements
            if (RuntimeSettings.Instance.useNetworkDataNamespace)
            {
                CodeNamespaceAscension.Imports.Add(new CodeNamespaceImport("Ascension.Networking.Data"));
                CodeNamespaceAscensionInternal.Imports.Add(new CodeNamespaceImport("Ascension.Networking.Data"));
            }

            CodeCompileUnit = new CodeCompileUnit();
            CodeCompileUnit.Namespaces.Add(CodeNamespace);
            CodeCompileUnit.Namespaces.Add(CodeNamespaceAscension);
            CodeCompileUnit.Namespaces.Add(CodeNamespaceAscensionInternal);

            // resets all data on the context/assets
            DecorateDefinitions();

            // create automagical asset objects
            CreateCommandObjects();

            // sort all states by inheritance
            SortStatesByHierarchy();

            // flatten state hierarchy
            DecorateProperties();

            // go through all of the our assets and set them up for compilation
            CreateCompilationModel();

            // assign class id to each asset
            AssignTypeIds();

            // first up is to compile the states
            EmitCodeObjectModel();

            // generate code
            GenerateSourceCode(file);
        }

        public FilterDefinition FindFilter(int index)
        {
            return Filters.First(x => x.Index == index);
        }

        public StateDecorator FindState(Guid guid)
        {
            return States.First(x => x.Definition.Guid == guid);
        }

        public ObjectDecorator FindStruct(Guid guid)
        {
            return Objects.First(x => x.Definition.Guid == guid);
        }

        public bool HasState(Guid guid)
        {
            return States.Any(x => x.Guid == guid);
        }

        public CodeTypeDeclaration DeclareInterface(string name, params string[] inherits)
        {
            return CodeNamespace.DeclareInterface(name, inherits);
        }

        public CodeTypeDeclaration DeclareStruct(string name)
        {
            return CodeNamespace.DeclareStruct(name);
        }

        public CodeTypeDeclaration DeclareClass(string name)
        {
            return CodeNamespace.DeclareClass(name);
        }

        private void DecorateDefinitions()
        {
            foreach (StateDefinition def in Project.States)
            {
                StateDecorator decorator;

                decorator = new StateDecorator(def);
                decorator.Generator = this;

                States.Add(decorator);
            }

            foreach (ObjectDefinition def in Project.Structs)
            {
                ObjectDecorator decorator;

                decorator = new ObjectDecorator(def);
                decorator.Generator = this;

                Objects.Add(decorator);
            }

            foreach (EventDefinition def in Project.Events)
            {
                EventDecorator decorator;

                decorator = new EventDecorator(def);
                decorator.Generator = this;

                Events.Add(decorator);
            }

            foreach (CommandDefinition def in Project.Commands)
            {
                CommandDecorator decorator;

                decorator = new CommandDecorator(def);
                decorator.Generator = this;

                Commands.Add(decorator);
            }
        }

        private ObjectDecorator CreateCommandObjectDefintion(AssetDecorator asset, string suffix)
        {
            ObjectDefinition def;

            def = new ObjectDefinition();
            def.Deleted = false;
            def.Enabled = true;
            def.Guid = Guid.NewGuid();
            def.Name = asset.Name + suffix;
            def.Project = asset.Definition.Project;

            ObjectDecorator decorator;

            decorator = new CommandObjectDecorator(def);
            decorator.Generator = this;

            return decorator;
        }

        private PropertyDefinition CreateObjectProperty(string name, ObjectDecorator propertyType)
        {
            PropertyDefinition def;

            def = new PropertyDefinition();
            def.Name = name;
            def.Replicated = true;
            def.Enabled = true;
            def.Deleted = false;
            def.PropertyType = new PropertyTypeObject
            {
                Context = Project,
                StructGuid = propertyType.Guid
            };

            return def;
        }

        private void CreateCommandObjects()
        {
            foreach (CommandDecorator command in Commands)
            {
                ObjectDecorator input;
                input = CreateCommandObjectDefintion(command, "Input");
                input.Definition.Properties = command.Definition.Input;

                ObjectDecorator result;
                result = CreateCommandObjectDefintion(command, "Result");
                result.Definition.Properties = command.Definition.Result;

                Objects.Add(input);
                Objects.Add(result);

                command.Properties = new List<PropertyDecorator>();
                command.Definition.Properties = new List<PropertyDefinition>
                {
                    CreateObjectProperty("Input", input),
                    CreateObjectProperty("Result", result)
                };
            }
        }

        private void AssignTypeIds()
        {
            uint typeId = 0;

            foreach (CommandDecorator decorator in Commands)
            {
                decorator.TypeId = ++typeId;
            }

            foreach (EventDecorator decorator in Events)
            {
                decorator.TypeId = ++typeId;
            }

            foreach (StateDecorator decorator in States)
            {
                decorator.TypeId = ++typeId;
            }

            foreach (ObjectDecorator decorator in Objects)
            {
                decorator.TypeId = ++typeId;
            }
        }

        private void SortStatesByHierarchy()
        {
            // sort states by inheritance
            States.Sort((a, b) =>
            {
                // if 'a' is a parent of 'b', then 'a' is smaller
                if (b.ParentList.Any(x => x.Definition.Guid == a.Definition.Guid))
                {
                    return -1;
                }

                // if 'b' is a parent 'a' of, then 'a' is larger
                if (a.ParentList.Any(x => x.Definition.Guid == b.Definition.Guid))
                {
                    return 1;
                }

                // neither, so order on guid
                return a.Definition.Guid.CompareTo(b.Definition.Guid);
            });
        }

        private void EmitCodeObjectModel()
        {
            foreach (ObjectDecorator s in Objects)
            {
                AssetCodeEmitter emitter;
                emitter = new AssetCodeEmitter();
                emitter.Decorator = s;
                emitter.Emit();
            }

            foreach (StateDecorator s in States)
            {
                StateCodeEmitter emitter;
                emitter = new StateCodeEmitter();
                emitter.Decorator = s;
                emitter.Emit();
            }

            foreach (EventDecorator d in Events)
            {
                EventCodeEmitter emitter;
                emitter = new EventCodeEmitter();
                emitter.Decorator = d;
                emitter.Emit();
            }

            foreach (CommandDecorator d in Commands)
            {
                CommandCodeEmitter emitter;
                emitter = new CommandCodeEmitter();
                emitter.Decorator = d;
                emitter.Emit();
            }

            EmitEventBaseClasses();
            EmitStateTypeIdLookup();
        }

        private void EmitStateTypeIdLookup()
        {
            CodeTypeDeclaration type;

            type = new CodeTypeDeclaration("StateSerializerTypeIds");
            type.TypeAttributes = TypeAttributes.Public | TypeAttributes.Abstract;

            foreach (StateDecorator s in States)
            {
                if (s.Definition.IsAbstract)
                {
                    continue;
                }

                CodeMemberField field = type.DeclareField("Ascension.Networking.UniqueId", s.NameInterface);
                field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                field.InitExpression =
                    new CodeSnippetExpression(string.Format("new Ascension.Networking.UniqueId({0})", s.Guid.ToByteArray().Join(", ")));
            }

            CodeNamespaceAscensionInternal.Types.Add(type);
        }

        private void EmitEventBaseClasses()
        {
            CodeTypeDeclaration globalListener = CodeNamespaceAscension.DeclareClass("GlobalEventListener");
            CodeTypeDeclaration entityListener = CodeNamespaceAscension.DeclareClass("EntityEventListener");
            CodeTypeDeclaration entityListenerGeneric = CodeNamespaceAscension.DeclareClass("EntityEventListener<TState>");

            globalListener.BaseTypes.Add("Ascension.Networking.GlobalEventListenerBase");
            entityListener.BaseTypes.Add("Ascension.Networking.EntityEventListenerBase");
            entityListenerGeneric.BaseTypes.Add("Ascension.Networking.EntityEventListenerBase<TState>");

            foreach (EventDecorator d in Events)
            {
                EmitEventMethodOverride(true, globalListener, d);
                EmitEventMethodOverride(false, entityListener, d);
                EmitEventMethodOverride(false, entityListenerGeneric, d);
            }
        }

        private void EmitEventMethodOverride(bool global, CodeTypeDeclaration type, EventDecorator d)
        {
            if (global && d.Definition.GlobalSenders == GlobalEventSenders.None)
            {
                return;
            }

            if (!global && d.Definition.EntitySenders == EntityEventSenders.None)
            {
                return;
            }

            type.BaseTypes.Add(d.ListenerInterface);

            type.DeclareMethod(typeof (void).FullName, "OnEvent", method => { method.DeclareParameter(d.Name, "evnt"); })
                .Attributes = MemberAttributes.Public;
        }

        private void DecorateProperties()
        {
            // Objects
            foreach (ObjectDecorator s in Objects)
            {
                s.Properties = PropertyDecorator.Decorate(s.Definition.Properties, s);
            }

            // Events
            foreach (EventDecorator d in Events)
            {
                d.Properties = PropertyDecorator.Decorate(d.Definition.Properties, d);
            }

            // Commands
            foreach (CommandDecorator d in Commands)
            {
                d.Properties = PropertyDecorator.Decorate(d.Definition.Properties, d);
            }

            // States
            foreach (StateDecorator s in States)
            {
                s.Properties = new List<PropertyDecorator>();

                // decorate and clone all parent properties, in order
                foreach (StateDecorator parent in s.ParentList)
                {
                    s.Properties.AddRange(PropertyDecorator.Decorate(parent.Definition.Properties, parent));
                }

                // decorate own properties
                s.Properties.AddRange(PropertyDecorator.Decorate(s.Definition.Properties, s));
            }
        }

        private void OrderStructsByDependancies()
        {
            List<ObjectDecorator> structs = Objects.Where(x => x.Dependencies.Count() == 0).ToList();
            List<ObjectDecorator> structsWithDeps = Objects.Where(x => x.Dependencies.Count() != 0).ToList();

            while (structsWithDeps.Count > 0)
            {
                for (int i = 0; i < structsWithDeps.Count; ++i)
                {
                    ObjectDecorator s = structsWithDeps[i];

                    if (s.Dependencies.All(x => structs.Any(y => y.Guid == x.Guid)))
                    {
                        // remove from deps list
                        structsWithDeps.RemoveAt(i);

                        // insert into structs list
                        structs.Add(s);

                        // decrement index counter
                        i -= 1;
                    }
                }
            }

            Objects = structs;
        }

        private void CreateCompilationModel()
        {
            // order all structs by their dependancies
            OrderStructsByDependancies();

            CountProperties(Objects);
            CountProperties(Events);
            CountProperties(Commands);
            CountProperties(States);
        }

        private void CountProperties<T>(IList<T> assets) where T : AssetDecorator
        {
            for (int i = 0; i < assets.Count; ++i)
            {
                AssetDecorator decorator;

                decorator = assets[i];
                decorator.CountObjects = 1;

                for (int p = 0; p < decorator.Properties.Count; ++p)
                {
                    if (decorator.Properties[p].RequiredObjects == 0)
                    {
                        decorator.Properties[p].OffsetObjects = 0;
                    }
                    else
                    {
                        decorator.Properties[p].OffsetObjects = decorator.CountObjects;
                    }

                    decorator.Properties[p].OffsetStorage = decorator.CountStorage;
                    decorator.Properties[p].OffsetProperties = decorator.CountProperties;

                    decorator.CountObjects += decorator.Properties[p].RequiredObjects;
                    decorator.CountStorage += decorator.Properties[p].RequiredStorage;
                    decorator.CountProperties += decorator.Properties[p].RequiredProperties;
                }
            }
        }

        private void GenerateUsingStatements(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("using UE = UnityEngine;");
            sb.AppendLine("using Encoding = System.Text.Encoding;");
            sb.AppendLine();
            //sb.AppendLine("using Ascension.Networking");
            //sb.AppendLine("using Ascension.Networking.Sockets;");
            //sb.AppendLine();
        }

        private void GenerateSourceCode(string file)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            GenerateUsingStatements(sb);

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();

            options.BlankLinesBetweenMembers = false;
            options.IndentString = "  ";

            provider.GenerateCodeFromCompileUnit(CodeCompileUnit, sw, options);

            sw.Flush();
            sw.Dispose();

            File.WriteAllText(file, sb.ToString());
        }
    }
}