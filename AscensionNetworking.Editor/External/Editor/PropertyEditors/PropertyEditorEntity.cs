using Ascension.Compiler;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorEntity : PropertyEditor<PropertyTypeEntity>
{
    protected override void Edit(bool array)
    {
        //DEditorGUI.WithLabel("Is Parent", () => {
        //  PropertyType.IsParent = EditorGUILayout.Toggle(PropertyType.IsParent); 
        //});
    }
}