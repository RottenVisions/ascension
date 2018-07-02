using Ascension.Compiler;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorString : PropertyEditor<PropertyTypeString>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.WithLabel("Encoding & Length", () =>
        {
            PropertyType.Encoding = (StringEncodings) EditorGUILayout.EnumPopup(PropertyType.Encoding);
            PropertyType.MaxLength =
                Mathf.Clamp(DEditorGUI.IntFieldOverlay(PropertyType.MaxLength, "Max Length (1 - 140)"), 1, 140);
        });
    }
}