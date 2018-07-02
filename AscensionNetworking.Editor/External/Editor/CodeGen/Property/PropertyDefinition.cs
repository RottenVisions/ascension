using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyDefinition
    {
        [ProtoIgnore] public int Adjust;
        [ProtoMember(6)] public PropertyAssetSettings AssetSettings;
        [ProtoMember(7)] public string Comment;
        [ProtoIgnore] public Project Context;
        [ProtoMember(10)] public bool Controller;
        [ProtoIgnore] public bool Deleted;
        [ProtoMember(3)] public bool Enabled;
        [ProtoMember(5)] public bool Expanded;
        [ProtoMember(8)] public int Filters;
        [ProtoIgnore] public bool IsArrayElement;
        [ProtoMember(1)] public string Name;
        [ProtoIgnore] public int Nudge;
        [ProtoMember(9)] public int Priority;
        [ProtoMember(2)] public PropertyType PropertyType;
        [ProtoMember(4)] public bool Replicated;
        [ProtoMember(11)] public ReplicationMode ReplicationMode;

        public PropertyStateSettings StateAssetSettings
        {
            get { return AssetSettings as PropertyStateSettings; }
        }

        public PropertyEventSettings EventAssetSettings
        {
            get { return AssetSettings as PropertyEventSettings; }
        }

        public PropertyCommandSettings CommandAssetSettings
        {
            get { return AssetSettings as PropertyCommandSettings; }
        }

        public void Oncreated()
        {
            Enabled = true;
            Expanded = true;

            Priority = 1;

            if (StateAssetSettings != null)
            {
                StateAssetSettings.SnapMagnitude = 10f;
            }

            PropertyType.OnCreated();
        }
    }
}