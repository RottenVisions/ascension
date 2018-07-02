using Ascension.Compiler;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorInteger : PropertyEditor<PropertyTypeInteger>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.WithLabel("Compression", () =>
        {
            PropertyType.CompressionEnabled = DEditorGUI.Toggle(PropertyType.CompressionEnabled);


            EditorGUI.BeginDisabledGroup(PropertyType.CompressionEnabled == false);

            PropertyType.MinValue = Mathf.Min(DEditorGUI.IntFieldOverlay(PropertyType.MinValue, "Min"),
                PropertyType.MaxValue - 1);
            PropertyType.MaxValue = Mathf.Max(DEditorGUI.IntFieldOverlay(PropertyType.MaxValue, "Max"),
                PropertyType.MinValue + 1);

            GUILayout.Label("Bits: " + PropertyType.BitsRequired, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));

            EditorGUI.EndDisabledGroup();
        });

        //DEditorGUI.WithLabel("Min Value", () => { PropertyType.MinValue = EditorGUILayout.IntField(PropertyType.MinValue); });
        //DEditorGUI.WithLabel("Max Value", () => { PropertyType.MaxValue = EditorGUILayout.IntField(PropertyType.MaxValue); });

        //DEditorGUI.WithLabel("Info", () => {
        //  EditorGUILayout.LabelField("Bits: " + BoltMath.BitsRequired(PropertyType.MaxValue - PropertyType.MinValue));
        //});

        //PropertyType.MinValue = Mathf.Min(PropertyType.MinValue, PropertyType.MaxValue - 1);
        //PropertyType.MaxValue = Mathf.Max(PropertyType.MaxValue, PropertyType.MinValue + 1);
    }
}