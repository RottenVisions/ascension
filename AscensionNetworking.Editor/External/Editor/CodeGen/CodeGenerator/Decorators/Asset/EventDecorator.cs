using System.Collections.Generic;

namespace Ascension.Compiler
{
    public class EventDecorator : AssetDecorator<EventDefinition>
    {
        public EventDecorator(EventDefinition def)
        {
            Definition = def;
        }

        public override string FactoryInterface
        {
            get { return "Ascension.Networking.IEventFactory"; }
        }

        public override string BaseClass
        {
            get { return "Ascension.Networking.Event"; }
        }

        public override bool EmitPropertyChanged
        {
            get { return false; }
        }

        public override List<PropertyDecorator> Properties { get; set; }

        public string ListenerInterface
        {
            get { return "I" + Name + "Listener"; }
        }
    }
}