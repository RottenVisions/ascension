using Ascension.Compiler;
using UnityEditor;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorVector : PropertyEditor<PropertyTypeVector>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

        DEditorGUI.WithLabel("Axes",
            () => { PropertyType.Selection = DEditorGUI.EditAxisSelection(PropertyType.Selection); });

        PropertyCommandSettings cmdSettings = Definition.CommandAssetSettings;
        PropertyStateSettings stateSettings = Definition.StateAssetSettings;

        if (Asset is StateDefinition)
        {
            DEditorGUI.WithLabel("Strict Comparison",
                () => { PropertyType.StrictEquality = EditorGUILayout.Toggle(PropertyType.StrictEquality); });

            DEditorGUI.WithLabel("Teleport Threshold", () =>
            {
                if (cmdSettings != null)
                {
                    cmdSettings.SnapMagnitude = EditorGUILayout.FloatField(cmdSettings.SnapMagnitude);
                }

                if (stateSettings != null)
                {
                    stateSettings.SnapMagnitude = EditorGUILayout.FloatField(stateSettings.SnapMagnitude);
                }
            });
        }

        DEditorGUI.WithLabel("Axis Compression",
            () => { DEditorGUI.EditAxes(PropertyType.Compression, PropertyType.Selection); });
    }
}