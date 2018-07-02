using System.Collections.Generic;

namespace Ascension.Compiler
{
    public class CommandObjectDecorator : ObjectDecorator
    {
        public CommandObjectDecorator(ObjectDefinition def)
            : base(def)
        {
        }

        public override bool EmitAsInterface
        {
            get { return true; }
        }

        public override string BaseInterface
        {
            get { return "Ascension.Networking.INetworkCommandData"; }
        }

        public override string BaseClassMeta
        {
            get { return "Ascension.Networking.NetworkObj_Meta"; }
        }

        public override string BaseClass
        {
            get { return "Ascension.Networking.NetworkCommand_Data"; }
        }
    }

    public class CommandDecorator : AssetDecorator<CommandDefinition>
    {
        public CommandDecorator(CommandDefinition def)
        {
            Definition = def;
        }

        public override string FactoryInterface
        {
            get { return "Ascension.Networking.ICommandFactory"; }
        }

        public override string BaseClass
        {
            get { return "Ascension.Networking.Command"; }
        }

        public override bool EmitPropertyChanged
        {
            get { return false; }
        }

        public override List<PropertyDecorator> Properties { get; set; }
    }
}