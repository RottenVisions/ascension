using Ascension.Compiler;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

public class PropertyEditorArray : PropertyEditor<PropertyTypeArray>
{
    protected override void Edit(bool array)
    {
        DEditorGUI.WithLabel("Element Type",
            () =>
            {
                PropertyType.ElementType = DEditorGUI.PropertyTypePopup(PropertyType.AllowedElementTypes,
                    PropertyType.ElementType);
            });

        DEditorGUI.WithLabel("Element Count",
            () => { PropertyType.ElementCount = Mathf.Max(2, EditorGUILayout.IntField(PropertyType.ElementCount)); });

        if (PropertyType.ElementType.HasSettings)
        {
            EditorGUILayout.BeginVertical();
            PropertyEditorRegistry.GetEditor(PropertyType.ElementType)
                .EditArrayElement(Asset, Definition, PropertyType.ElementType);
            EditorGUILayout.EndVertical();
        }
    }
}