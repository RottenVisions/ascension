using System.CodeDom;
using System.Diagnostics;
using System.Linq;

namespace Ascension.Compiler
{
    public class EventCodeEmitter : AssetCodeEmitter<EventDecorator>
    {
        public override void Emit()
        {
            base.Emit();
            EmitListenerInterface();
        }

        private void DeclareObsolete(CodeMemberMethod method, string name)
        {
            if (name == "Raise")
            {
                method.DeclareObsolete("This method is obsolete, use 'Create' instead");
            }
        }

        private void EmitCreate(CodeTypeDeclaration type, bool inherited, string name)
        {
            if (Decorator.Definition.EntitySenders != EntityEventSenders.None)
            {
                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.AscensionEntity", "entity");
                    method.Statements.Expr("return Create(entity, Ascension.Networking.EntityTargets.Everyone)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.AscensionEntity", "entity");
                    method.DeclareParameter("Ascension.Networking.EntityTargets", "targets");

                    method.Statements.Expr("if (!entity) throw new System.ArgumentNullException(\"entity\")");
                    method.Statements.Expr(
                        "if (!entity.IsAttached) throw new Ascension.Networking.AscensionException(\"You can not raise events on entities which are not attached\")");

                    switch (Decorator.Definition.EntitySenders)
                    {
                        case EntityEventSenders.OnlyOwner:
                            method.Statements.Expr(
                                "if (!entity.IsOwner) throw new Ascension.Networking.AscensionException(\"You are not the owner of {{0}}, you can not raise this event\", entity)");
                            break;
                        case EntityEventSenders.OnlyController:
                            method.Statements.Expr(
                                "if (!entity.HasControl) throw new Ascension.Networking.AscensionException(\"You are not the controller of {{0}}, you can not raise this event\", entity)");
                            break;
                    }

                    method.Statements.Expr("{0} evt", Decorator.Definition.Name);
                    method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
                    method.Statements.Expr("evt.Targets = (byte) targets", Decorator.Definition.EntityTargets);
                    method.Statements.Expr("evt.TargetEntity = entity.entity");
                    method.Statements.Expr("evt.Reliability = Ascension.Networking.ReliabilityModes.Unreliable");
                    method.Statements.Expr("return evt");
                });
            }

            if (Decorator.Definition.GlobalSenders != GlobalEventSenders.None)
            {
                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                    method.DeclareParameter(typeof (byte).FullName, "targets");
                    method.DeclareParameter("Ascension.Networking.Connection", "connection");
                    method.DeclareParameter("Ascension.Networking.ReliabilityModes", "reliability");

                    switch (Decorator.Definition.GlobalSenders)
                    {
                        case GlobalEventSenders.OnlyServer:
                            method.Statements.Expr(
                                "if (!Ascension.Networking.Core.IsServer) throw new Ascension.Networking.AscensionException(\"You are not the server of, you can not raise this event\")");
                            break;
                        case GlobalEventSenders.OnlyClients:
                            method.Statements.Expr(
                                "if (!Ascension.Networking.Core.IsClient) throw new Ascension.Networking.AscensionException(\"You are not the a client, you can not raise this event\")");
                            break;
                    }

                    method.Statements.Expr("{0} evt", Decorator.Definition.Name);
                    method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
                    method.Statements.Expr("evt.Targets = targets");
                    method.Statements.Expr("evt.TargetConnection = connection");
                    method.Statements.Expr("evt.Reliability = reliability");
                    method.Statements.Expr("return evt");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.GlobalTargets", "targets");
                    method.Statements.Expr("return Create((byte) targets, null, Ascension.Networking.ReliabilityModes.ReliableOrdered)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.GlobalTargets", "targets");
                    method.DeclareParameter("Ascension.Networking.ReliabilityModes", "reliability");
                    method.Statements.Expr("return Create((byte) targets, null, reliability)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.Connection", "connection");
                    method.Statements.Expr(
                        "return Create(Ascension.Networking.Event.GLOBAL_SPECIFIC_CONNECTION, connection, Ascension.Networking.ReliabilityModes.ReliableOrdered)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.Connection", "connection");
                    method.DeclareParameter("Ascension.Networking.ReliabilityModes", "reliability");
                    method.Statements.Expr(
                        "return Create(Ascension.Networking.Event.GLOBAL_SPECIFIC_CONNECTION, connection, reliability)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.Statements.Expr(
                        "return Create(Ascension.Networking.Event.GLOBAL_EVERYONE, null, Ascension.Networking.ReliabilityModes.ReliableOrdered)");
                });

                type.DeclareMethod(Decorator.Definition.Name, name, method =>
                {
                    DeclareObsolete(method, name);

                    method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                    method.DeclareParameter("Ascension.Networking.ReliabilityModes", "reliability");
                    method.Statements.Expr("return Create(Ascension.Networking.Event.GLOBAL_EVERYONE, null, reliability)");
                });
            }
        }

        protected override void EmitObjectMembers(CodeTypeDeclaration type, bool inherited)
        {
            base.EmitObjectMembers(type, inherited);

            type.DeclareMethod(typeof (string).FullName, "ToString", method =>
            {
                method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

                string name = Decorator.Definition.Name;
                string properties =
                    Decorator.Properties.Select((p, i) => " " + p.Definition.Name + "={{" + i + "}}").Join("");
                string arguments = Decorator.Properties.Select((p, i) => ", this." + p.Definition.Name).Join("");
                method.Statements.Expr("return System.String.Format(\"[" + name + properties + "]\"" + arguments + ")");
            });

            EmitCreate(type, inherited, "Raise");
            EmitCreate(type, inherited, "Create");
        }

        private void EmitListenerInterface()
        {
            CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.ListenerInterface);

            type.DeclareMethod(typeof (void), "OnEvent", method => { method.DeclareParameter(Decorator.Name, "ev"); });
        }

        protected override void EmitFactory()
        {
            base.EmitFactory();

            MetaType.DeclareMethod(typeof (void), "Dispatch", method =>
            {
                method.DeclareParameter("Ascension.Networking.Event", "ev");
                method.DeclareParameter(typeof (object).FullName, "target");

                DomBlock block = new DomBlock(method.Statements);

                var tmp = block.TempVar();

                block.Stmts.Expr("{0} {1} = target as {0}", Decorator.ListenerInterface, tmp);
                block.Stmts.Expr("if ({0} != null) {{ {0}.OnEvent(({1})ev); }}", tmp, Decorator.Name);
            }).PrivateImplementationType = new CodeTypeReference(Decorator.FactoryInterface);
        }
    }
}