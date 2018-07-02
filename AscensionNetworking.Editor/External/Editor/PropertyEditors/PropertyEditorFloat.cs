using Ascension.Compiler;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

        if (Asset is StateDefinition)
        {
            if (Definition.StateAssetSettings.SmoothingAlgorithm == SmoothingAlgorithms.Interpolation)
            {
                DEditorGUI.WithLabel("Interpolation Mode",
                    () =>
                    {
                        PropertyType.IsAngle = DEditorGUI.ToggleDropdown("As Angle", "As Float", PropertyType.IsAngle);
                    });
            }
        }

        DEditorGUI.WithLabel("Compression",
            () => { PropertyType.Compression = DEditorGUI.EditFloatCompression(PropertyType.Compression); });
    }
}