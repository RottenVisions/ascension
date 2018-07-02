using Ascension.Compiler;
using UnityEditor;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.WithLabel("Space",
            () => { PropertyType.Space = (TransformSpaces) EditorGUILayout.EnumPopup(PropertyType.Space); });

        DEditorGUI.EditSmoothingAlgorithm(Asset, Definition);

        DEditorGUI.Header("Position", "position");

        DEditorGUI.WithLabel("Axes",
            () => { PropertyType.PositionSelection = DEditorGUI.EditAxisSelection(PropertyType.PositionSelection); });

        if (PropertyType.PositionSelection != AxisSelections.Disabled)
        {
            if (Asset is StateDefinition)
            {
                DEditorGUI.WithLabel("Strict Comparison",
                    () =>
                    {
                        PropertyType.PositionStrictCompare = EditorGUILayout.Toggle(PropertyType.PositionStrictCompare);
                    });

                DEditorGUI.WithLabel("Teleport Threshold",
                    () =>
                    {
                        Definition.StateAssetSettings.SnapMagnitude =
                            EditorGUILayout.FloatField(Definition.StateAssetSettings.SnapMagnitude);
                    });
            }

            DEditorGUI.WithLabel("Compression",
                () => { DEditorGUI.EditAxes(PropertyType.PositionCompression, PropertyType.PositionSelection); });
        }


        DEditorGUI.Header("Rotation", "rotation");

        DEditorGUI.WithLabel("Axes",
            () => { PropertyType.RotationSelection = DEditorGUI.EditAxisSelection(PropertyType.RotationSelection); });

        if (PropertyType.RotationSelection != AxisSelections.Disabled)
        {
            if (Asset is StateDefinition)
            {
                DEditorGUI.WithLabel("Strict Comparison",
                    () =>
                    {
                        PropertyType.RotationStrictCompare = EditorGUILayout.Toggle(PropertyType.RotationStrictCompare);
                    });
            }

            bool quaternion = PropertyType.RotationSelection == AxisSelections.XYZ;

            DEditorGUI.WithLabel(quaternion ? "Compression (Quaternion)" : "Compression (Euler)", () =>
            {
                if (quaternion)
                {
                    PropertyType.RotationCompressionQuaternion =
                        DEditorGUI.EditFloatCompression(PropertyType.RotationCompressionQuaternion);
                }
                else
                {
                    DEditorGUI.EditAxes(PropertyType.RotationCompression, PropertyType.RotationSelection);
                }
            });
        }
    }
}