using Ascension.Compiler;
using UnityEditor;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorQuaternion : PropertyEditor<PropertyTypeQuaternion>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

        DEditorGUI.WithLabel("Axes",
            () => { PropertyType.Selection = DEditorGUI.EditAxisSelection(PropertyType.Selection); });

        if (Asset is StateDefinition)
        {
            DEditorGUI.WithLabel("Strict Comparison",
                () => { PropertyType.StrictEquality = EditorGUILayout.Toggle(PropertyType.StrictEquality); });
        }

        if (PropertyType.Selection != AxisSelections.Disabled)
        {
            bool quaternion = PropertyType.Selection == AxisSelections.XYZ;

            DEditorGUI.WithLabel(quaternion ? "Quaternion Compression" : "Axis Compression", () =>
            {
                if (quaternion)
                {
                    PropertyType.QuaternionCompression =
                        DEditorGUI.EditFloatCompression(PropertyType.QuaternionCompression);
                }
                else
                {
                    DEditorGUI.EditAxes(PropertyType.EulerCompression, PropertyType.Selection);
                }
            });
        }
    }
}