using System;
using System.CodeDom;
using System.Collections.Generic;
using Ascension.Networking;

namespace Ascension.Compiler
{
    public abstract class PropertyCodeEmitter
    {
        public PropertyDecorator Decorator;

        public CodeGenerator Generator
        {
            get { return Decorator.Generator; }
        }

        public virtual string StorageField
        {
            get { return GetType().Name.Replace("PropertyCodeEmitter", ""); }
        }

        public virtual string SerializerClassName
        {
            get { return Decorator.PropertyClassName; }
        }

        public virtual bool AllowSetter
        {
            get { return true; }
        }

        public virtual bool VerifyModify
        {
            get { return true; }
        }

        public virtual void AddSettings(CodeExpression expr, CodeStatementCollection statements)
        {
        }

        public virtual void EmitObjectMembers(CodeTypeDeclaration type)
        {
            EmitSimplePropertyMembers(type, new CodeSnippetExpression("Storage"), null, true);
        }

        public void EmitSimplePropertyMembers(CodeTypeDeclaration type, CodeSnippetExpression storage,
            CodeTypeReference interfaceType, bool changed)
        {
            EmitSimplePropertyMembers(type, storage, interfaceType, changed, Decorator.Definition.Name);
        }

        public void EmitSimplePropertyMembers(CodeTypeDeclaration type, CodeSnippetExpression storage,
            CodeTypeReference interfaceType, bool changed, string name)
        {
            CodeIndexerExpression index = new CodeIndexerExpression(storage.Field("Values"),
                "this.OffsetStorage + {0}".Expr(Decorator.OffsetStorage));

            // getter method
            Action<CodeStatementCollection> getter = get =>
            {
                get.Add(
                    new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(index, StorageField)
                        )
                    );
            };

            // setter method
            Action<CodeStatementCollection> setter = set =>
            {
                PropertyStateSettings s = Decorator.Definition.StateAssetSettings;
                if (s != null && VerifyModify)
                {
                    EmitAllowedCheck(set);
                }

                // allows emission of a validator snippet
                EmitSetPropertyValidator(set, type, storage, interfaceType, changed, name);

                if (changed)
                {
                    set.Add("{0} oldValue".Expr(Decorator.ClrType));
                    set.Add("oldValue".Expr().Assign(new CodeFieldReferenceExpression(index, StorageField)));
                }

                set.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(index, StorageField),
                    new CodeVariableReferenceExpression("value")
                    ));

                if (changed)
                {
                    string diff = Decorator.Definition.PropertyType.StrictCompare ? "Diff_Strict" : "Diff";
                    set.If("Ascension.Networking.NetworkValue.{0}(oldValue, value)".Expr(diff),
                        body => { EmitPropertyChanged(body, storage); });
                }
            };

            if (!AllowSetter)
            {
                setter = null;
            }

            CodeMemberProperty property = type.DeclareProperty(Decorator.ClrType, name, getter, setter);
            property.PrivateImplementationType = interfaceType;
            property.Attributes = Decorator.Attributes;
        }

        protected virtual void EmitSetPropertyValidator(CodeStatementCollection stmts, CodeTypeDeclaration type,
            CodeSnippetExpression storage, CodeTypeReference interfaceType, bool changed, string name)
        {
        }

        protected void EmitAllowedCheck(CodeStatementCollection stmts)
        {
#if DEBUG
            string error = null;

            switch (Decorator.Definition.ReplicationMode)
            {
                case ReplicationMode.EveryoneExceptController:
                    error =
                        @"var en = this.RootState.Entity; if(!en.IsOwner && !en.HasControl) {{ Ascension.Networking.NetLog.Error(""Only the owner and controller can modify: '{0}'""); return; }}";
                    break;

                case ReplicationMode.Everyone:
                    error =
                        @"var en = this.RootState.Entity; if(!en.IsOwner) {{ Ascension.Networking.NetLog.Error(""Only the owner can modify: '{0}'""); return; }}";
                    break;

                case ReplicationMode.OnlyOwnerAndController:
                    error =
                        @"var en = this.RootState.Entity; if(!en.IsOwner && en.HasControl) {{ Ascension.Networking.NetLog.Error(""Controller is not allowed to modify '{0}'""); return; }}";
                    break;
            }


            if (error != null)
            {
                stmts.Expr(error, Decorator.Definition.Name);
            }
#endif
        }

        public void EmitPropertyChanged(CodeStatementCollection stmt, CodeExpression storage)
        {
            if (Decorator.DefiningAsset.EmitPropertyChanged)
            {
                stmt.Add(storage.Call("PropertyChanged", "this.OffsetProperties + {0}".Expr(Decorator.OffsetProperties)));
            }
        }

        public virtual void EmitMetaSetup(DomBlock block)
        {
            Offsets offsets = new Offsets();
            offsets.OffsetStorage = "{0} /*required-storage:{1}*/".Expr(Decorator.OffsetStorage,
                Decorator.RequiredStorage);
            offsets.OffsetProperties = "{0} /*required-properties:{1}*/".Expr(Decorator.OffsetProperties,
                Decorator.RequiredProperties);
            offsets.OffsetObjects = "{0} /*required-objects:{1}*/".Expr(Decorator.OffsetObjects,
                Decorator.RequiredObjects);

            EmitMetaSetup(block, offsets);
        }

        public virtual void EmitMetaSetup(DomBlock block, Offsets offsets)
        {
            EmitMetaSetup(block, offsets, null);
        }

        public virtual void EmitMetaSetup(DomBlock block, Offsets offsets, CodeExpression indexExpression)
        {
            var tmp = block.Stmts.Var(SerializerClassName, block.TempVar());

            block.Stmts.Assign(tmp, SerializerClassName.New());
            block.Stmts.Assign(tmp.Field("PropertyMeta"), "this".Expr());

            EmitAddSettings(tmp, block.Stmts, offsets);

            block.Stmts.Add("this".Expr()
                .Call("AddProperty", offsets.OffsetProperties, offsets.OffsetObjects, tmp,
                    indexExpression ?? (-1).Literal()));
        }

        public virtual void EmitObjectSetup(DomBlock block)
        {
            Offsets offsets = new Offsets();
            offsets.OffsetStorage = "offsets.OffsetStorage + {0} /*required-storage:{1}*/".Expr(
                Decorator.OffsetStorage, Decorator.RequiredStorage);
            offsets.OffsetObjects = "offsets.OffsetObjects + {0} /*required-object:{1}*/".Expr(Decorator.OffsetObjects,
                Decorator.RequiredObjects);
            offsets.OffsetProperties =
                "offsets.OffsetProperties + {0} /*required-properties:{1}*/".Expr(Decorator.OffsetProperties,
                    Decorator.RequiredProperties);

            EmitObjectSetup(block, offsets);
        }

        public virtual void EmitObjectSetup(DomBlock block, Offsets offsets)
        {
            Assert.True(Decorator.RequiredObjects == 0);
            block.Stmts.Comment("EMPTY");

            // add storage
            //block.Stmts.Expr("{0}.AddSerializer({1})", group, this.Decorator.RequiredStorage);
        }

        public void EmitInterpolationSettings(CodeExpression expr, CodeStatementCollection statements)
        {
            PropertyStateSettings s = Decorator.Definition.StateAssetSettings;
            PropertyCommandSettings c = Decorator.Definition.CommandAssetSettings;

            if (s != null)
            {
                if (s.SmoothingAlgorithm != SmoothingAlgorithms.None)
                {
                    statements.Add(expr.Call("Settings_Interpolation", s.SnapMagnitude.Literal(), true.Literal()));
                }
            }

            if (c != null)
            {
                if (c.SmoothCorrection)
                {
                    statements.Add(expr.Call("Settings_Interpolation", c.SnapMagnitude.Literal(), false.Literal()));
                }
            }
        }

        public CodeExpression CreateFloatCompressionExpression(FloatCompression c, bool enabled)
        {
            if (c == null)
            {
                c = FloatCompression.Default();
            }

            if (enabled)
            {
                if (c.Enabled)
                {
                    return "Ascension.Networking.PropertyFloatCompressionSettings.Create({0}, {1}f, {2}f, {3}f)".Expr(c.BitsRequired,
                        c.Shift, c.Pack, c.Read);
                }
                return "Ascension.Networking.PropertyFloatCompressionSettings.Create()".Expr();
            }
            return "default(Ascension.Networking.PropertyFloatCompressionSettings)".Expr();
        }

        public List<CodeExpression> CreateAxisCompressionExpression(FloatCompression[] axes, AxisSelections selection)
        {
            if (axes == null)
            {
                selection = AxisSelections.XYZ;
            }

            List<CodeExpression> args = new List<CodeExpression>();
            args.Add(CreateFloatCompressionExpression(axes[Axis.X], (selection & AxisSelections.X) == AxisSelections.X));
            args.Add(CreateFloatCompressionExpression(axes[Axis.Y], (selection & AxisSelections.Y) == AxisSelections.Y));
            args.Add(CreateFloatCompressionExpression(axes[Axis.Z], (selection & AxisSelections.Z) == AxisSelections.Z));
            return args;
        }

        public void EmitFloatSettings(CodeExpression expr, CodeStatementCollection statements, FloatCompression c)
        {
            statements.Call(expr, "Settings_Float", CreateFloatCompressionExpression(c, true));
        }

        public void EmitVectorSettings(CodeExpression expr, CodeStatementCollection statements, FloatCompression[] axes,
            AxisSelections selection, bool strictCompare)
        {
            if (selection != AxisSelections.Disabled)
            {
                List<CodeExpression> exprs = CreateAxisCompressionExpression(axes, selection);
                exprs.Add(strictCompare.Literal());

                statements.Call(expr, "Settings_Vector", exprs.ToArray());
            }
        }

        public void EmitQuaternionSettings(CodeExpression expr, CodeStatementCollection statements,
            FloatCompression[] axes, FloatCompression quaternion, AxisSelections selection, bool strictCompare)
        {
            if (selection != AxisSelections.Disabled)
            {
                if (axes == null || quaternion == null || selection == AxisSelections.XYZ)
                {
                    statements.Call(expr, "Settings_Quaternion", CreateFloatCompressionExpression(quaternion, true),
                        strictCompare.Literal());
                }
                else
                {
                    List<CodeExpression> exprs = CreateAxisCompressionExpression(axes, selection);
                    exprs.Add(strictCompare.Literal());

                    statements.Call(expr, "Settings_QuaternionEuler", exprs.ToArray());
                }
            }
        }

        public void EmitInitObject(string type, DomBlock block, Offsets offsets, params CodeExpression[] ctorArguments)
        {
            var tmp = block.Stmts.Var(type, block.TempVar());
            block.Stmts.Add(tmp.Assign(type.New(ctorArguments)));
            block.Stmts.Add(tmp.Call("Init", Decorator.Definition.Name.Literal(), "obj.Root".Expr(),
                "Ascension.Networking.NetworkObj_Meta.Offsets".New(offsets.OffsetProperties, offsets.OffsetStorage,
                    offsets.OffsetObjects)));
        }

        public void EmitAddSettings(CodeExpression expr, CodeStatementCollection statements, Offsets offsets)
        {
            // fix for transfer from old system
            if (Decorator.Definition.Controller)
            {
                Decorator.Definition.ReplicationMode = ReplicationMode.Everyone;
            }

            int filters = 0;

            switch (Decorator.Definition.ReplicationMode)
            {
                case ReplicationMode.Everyone:
                    filters |= (1 << 30);
                    filters |= (1 << 31);
                    break;

                case ReplicationMode.EveryoneExceptController:
                    filters |= (1 << 30);
                    break;

                case ReplicationMode.OnlyOwnerAndController:
                    filters |= (1 << 31);
                    break;
            }

            statements.Call(expr, "Settings_Property",
                Decorator.Definition.Name.Literal(),
                Decorator.Definition.Priority.Literal(),
                filters.Literal()
                );

            statements.Call(expr, "Settings_Offsets",
                offsets.OffsetProperties,
                offsets.OffsetStorage
                );

            // mecanim for states settings
            if ((Decorator.DefiningAsset is StateDecorator) && Decorator.Definition.PropertyType.MecanimApplicable)
            {
                PropertyStateSettings s = Decorator.Definition.StateAssetSettings;

                statements.Call(expr, "Settings_Mecanim",
                    s.MecanimMode.Literal(),
                    s.MecanimDirection.Literal(),
                    s.MecanimDamping.Literal(),
                    s.MecanimLayer.Literal()
                    );
            }

            // collecting property specific settings
            AddSettings(expr, statements);
        }

        public void EmitSimpleIntefaceMember(CodeTypeDeclaration type, bool get, bool set)
        {
            if (get && set)
            {
                type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, _ => { }, _ => { });
            }
            else if (get)
            {
                type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, _ => { }, null);
            }
            else if (set)
            {
                type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, null, _ => { });
            }
        }

        public static PropertyCodeEmitter Create(PropertyDecorator decorator)
        {
            PropertyCodeEmitter emitter;

            emitter = decorator.CreateEmitter();
            emitter.Decorator = decorator;

            return emitter;
        }

        public struct Offsets
        {
            public CodeExpression OffsetObjects;
            public CodeExpression OffsetProperties;
            public CodeExpression OffsetStorage;
        }
    }

    public abstract class PropertyCodeEmitter<T> : PropertyCodeEmitter where T : PropertyDecorator
    {
        public new T Decorator
        {
            get { return (T) base.Decorator; }
        }
    }
}