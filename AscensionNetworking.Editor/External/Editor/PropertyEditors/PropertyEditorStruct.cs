using System.Linq;
using Ascension.Compiler;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorStruct : PropertyEditor<PropertyTypeObject>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.WithLabel("Object Type",
            () =>
            {
                PropertyType.StructGuid = DEditorGUI.AssetPopup(
                    AscensionWindow.Project.Structs.Cast<AssetDefinition>(), PropertyType.StructGuid, new[] {Asset.Guid});
            });
    }
}