using System.Linq;
using Ascension.Networking.Physics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AscensionHitboxBody))]
public class AscensionHitboxBodyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Find Hitboxes", EditorStyles.miniButton))
        {
            AscensionHitboxBody hbtarget = (AscensionHitboxBody)target;

            hbtarget.Hitboxes = hbtarget.GetComponentsInChildren<AscensionHitbox>().Where(x => x.type != AscensionHitboxType.Proximity).ToArray();
            hbtarget.Proximity = hbtarget.GetComponentsInChildren<AscensionHitbox>().FirstOrDefault(x => x.type == AscensionHitboxType.Proximity);

            EditorUtility.SetDirty(hbtarget);
        }
    }
}
