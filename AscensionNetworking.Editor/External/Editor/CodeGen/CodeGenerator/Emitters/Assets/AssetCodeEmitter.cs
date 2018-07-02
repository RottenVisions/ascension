using System.CodeDom;
using System.Reflection;
using Ascension.Networking;

namespace Ascension.Compiler
{
    public class AssetCodeEmitter
    {
        public AssetDecorator Decorator;
        public CodeTypeDeclaration InterfaceType;
        public CodeTypeDeclaration MetaType;
        public CodeTypeDeclaration ObjectType;

        public CodeGenerator Generator
        {
            get { return Decorator.Generator; }
        }

        protected virtual void EmitInterface()
        {
            if (Decorator.EmitAsInterface)
            {
                InterfaceType = Generator.DeclareInterface(Decorator.NameInterface);
                InterfaceType.TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface;
                InterfaceType.BaseTypes.Add(Decorator.BaseInterface);

                EmitObjectMembers(InterfaceType, false);
            }
        }

        protected virtual void EmitObject()
        {
            ObjectType = Generator.DeclareClass(Decorator.Name);
            ObjectType.BaseTypes.Add(Decorator.BaseClass);
            //Needed to change the emitted code to be 'public' instead of 'internal'
            ObjectType.Attributes = MemberAttributes.Public;

            if (Decorator.EmitAsInterface)
            {
                ObjectType.BaseTypes.Add(Decorator.NameInterface);
                ObjectType.TypeAttributes = TypeAttributes.Public;
            }
            else
            {
                ObjectType.TypeAttributes = TypeAttributes.Public;
            }

            ObjectType.DeclareConstructor(ctor =>
            {
                ctor.BaseConstructorArgs.Add(Decorator.NameMeta.Expr().Field("Instance"));
                EmitObjectCtor(ctor);
            });

            EmitObjectMembers(ObjectType, true);
        }

        protected virtual void EmitObjectMembers(CodeTypeDeclaration type, bool inherited)
        {
            for (int i = 0; i < Decorator.Properties.Count; ++i)
            {
                if (inherited || ReferenceEquals(Decorator.Properties[i].DefiningAsset, Decorator))
                {
                    PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitObjectMembers(type);
                }
            }
        }

        protected virtual void EmitObjectCtor(CodeConstructor ctor)
        {
        }

        protected virtual void EmitMeta()
        {
            MetaType = Generator.DeclareClass(Decorator.NameMeta);
            //MetaType.TypeAttributes = TypeAttributes.NestedAssembly;
            //Needed to change the emitted code to be 'public' instead of 'internal'
            MetaType.Attributes = MemberAttributes.Public;
            MetaType.TypeAttributes = TypeAttributes.Public;

            MetaType.BaseTypes.Add(Decorator.BaseClassMeta);
            MetaType.BaseTypes.Add(Decorator.FactoryInterface);

            //MetaType.DeclareField(Decorator.NameMeta, "Instance").Attributes = MemberAttributes.Static |
            //                                                                   MemberAttributes.Assembly;

            MetaType.DeclareField(Decorator.NameMeta, "Instance").Attributes = MemberAttributes.Static |
                                                                               MemberAttributes.Public;

            MetaType.DeclareConstructorStatic(ctor =>
            {
                ctor.Statements.Add("Instance".Expr().Assign(Decorator.NameMeta.New()));
                ctor.Statements.Add("Instance".Expr().Call("InitMeta"));

                EmitMetaStaticCtor(ctor);
            });

            // initialize object
            MetaType.DeclareMethod(typeof (void).FullName, "InitObject", method =>
            {
                //method.Attributes = MemberAttributes.Assembly | MemberAttributes.Override;

                method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

                method.DeclareParameter("Ascension.Networking.NetworkObj", "obj");
                method.DeclareParameter("Ascension.Networking.NetworkObj_Meta.Offsets", "offsets");

                DomBlock block;
                block = new DomBlock(method.Statements);

                for (int i = 0; i < Decorator.Properties.Count; ++i)
                {
                    block.Stmts.Comment("");
                    block.Stmts.Comment("Property: " + Decorator.Properties[i].Definition.Name);
                    PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitObjectSetup(block);
                }

                EmitObjectInit(method);
            });

            // initialize meta class
            MetaType.DeclareMethod(typeof (void).FullName, "InitMeta", method =>
            {
                //method.Attributes = MemberAttributes.Assembly | MemberAttributes.Override;

                method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

                DomBlock block;

                block = new DomBlock(method.Statements);
                block.Stmts.Comment("Setup fields");
                block.Stmts.Expr("this.TypeId = new Ascension.Networking.TypeId({0})", Decorator.TypeId);
                block.Stmts.Expr("this.CountStorage = {0}", Decorator.CountStorage);
                block.Stmts.Expr("this.CountObjects = {0}", Decorator.CountObjects);
                block.Stmts.Expr("this.CountProperties = {0}", Decorator.CountProperties);
                block.Stmts.Expr("this.Properties = new Ascension.Networking.NetworkPropertyInfo[{0}]", Decorator.CountProperties);

                EmitMetaInit(method);

                for (int i = 0; i < Decorator.Properties.Count; ++i)
                {
                    block.Stmts.Comment("");
                    block.Stmts.Comment("Property: " + Decorator.Properties[i].Definition.Name);
                    PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitMetaSetup(block);
                }

                block.Stmts.Expr("base.InitMeta()");
            });

            // emit factory interface
            EmitFactory();
        }

        protected virtual void EmitMetaStaticCtor(CodeTypeConstructor ctor)
        {
        }

        protected virtual void EmitMetaInit(CodeMemberMethod method)
        {
        }

        protected virtual void EmitObjectInit(CodeMemberMethod method)
        {
        }

        protected virtual void EmitFactory()
        {
            CodeTypeReference factoryInterface = new CodeTypeReference("Ascension.Networking.IFactory");

            MetaType.DeclareProperty("Ascension.Networking.TypeId", "TypeId", get => { get.Expr("return TypeId"); })
                .PrivateImplementationType = factoryInterface;

            MetaType.DeclareProperty("Ascension.Networking.UniqueId", "TypeKey",
                get =>
                {
                    get.Expr("return new Ascension.Networking.UniqueId({0})", Decorator.Definition.Guid.ToByteArray().Join(", "));
                })
                .PrivateImplementationType = factoryInterface;

            MetaType.DeclareProperty("System.Type", "TypeObject",
                get =>
                {
                    get.Expr("return typeof({0})", Decorator.EmitAsInterface ? Decorator.NameInterface : Decorator.Name);
                }).PrivateImplementationType = factoryInterface;

            MetaType.DeclareMethod(typeof (object).FullName, "Create",
                methoid => { methoid.Statements.Expr("return new {0}()", Decorator.Name); }).PrivateImplementationType =
                factoryInterface;
        }

        public virtual void Emit()
        {
            EmitInterface();
            EmitObject();
            EmitMeta();
        }
    }

    public abstract class AssetCodeEmitter<T> : AssetCodeEmitter where T : AssetDecorator
    {
        public new T Decorator
        {
            get { return (T) base.Decorator; }
            set { base.Decorator = value; }
        }
    }
}