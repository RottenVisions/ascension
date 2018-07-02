using System;
using System.Linq;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;
using DEditorGUI = Ascension.Tools.EditorGUI;

[CustomEditor(typeof(AscensionEntity))]
public class AscensionEntityEditor : Editor
{
    static string[] serializerNames;
    static UniqueId[] serializerIds;
    static ISerializerFactory[] serializerFactories;

    private AscensionEntity entity;

    static AscensionEntityEditor()
    {
        serializerFactories =
          typeof(ISerializerFactory)
            .FindInterfaceImplementations()
            .Select(x => Activator.CreateInstance(x))
            .Cast<ISerializerFactory>()
            .ToArray();

        serializerNames =
          new string[] { "NOT ASSIGNED" }
            .Concat(serializerFactories.Select(x => x.TypeObject.Name))
            .ToArray();

        serializerIds =
          serializerFactories
            .Select(x => x.TypeKey)
            .ToArray();
    }

    void HelpBox(string text)
    {
        RuntimeSettings settings = RuntimeSettings.Instance;

        if (settings.showAscensionEntityHints)
        {
            EditorGUILayout.HelpBox(text, MessageType.Info);
        }
    }

    void OnEnable()
    {
        entity = (AscensionEntity)target;
    }

    public override void OnInspectorGUI()
    {
        RuntimeSettings settings = RuntimeSettings.Instance;

        GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
        s.normal.textColor = new Color(0.47f, 0.87f, 0.47f, 75);

        AscensionEntity entity = (AscensionEntity)target;

        //GUILayout.BeginHorizontal();
        //GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128, 64, 64, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)), Resources.Load("AscensionLogo") as Texture2D);
        //GUILayout.EndHorizontal();

        GUILayout.Label("Prefab & State", s);

        DrawPrefabInfo(entity);
        EditState(entity);

        GUILayout.Label("Settings", s);

        entity.updateRate = EditorGUILayout.IntField("Replication Rate", entity.updateRate);
        HelpBox("Controls how often this entity should be considered for replication. 1 = Every packet, 2 = Every other packet, etc.");

        entity.persistThroughSceneLoads = EditorGUILayout.Toggle("Persistent", entity.persistThroughSceneLoads);
        HelpBox("If enabled Ascension will not destroy this object when a new scene is loaded through AscensionNetwork.LoadScene().");

        entity.alwaysProxy = EditorGUILayout.Toggle("Always Replicate", entity.alwaysProxy);
        HelpBox("If enabled Ascension will always replicate this entity and its state, even when operations that normally would block replication is taking place (example: loading a scene).");

        entity.allowFirstReplicationWhenFrozen = EditorGUILayout.Toggle("Proxy When Frozen", entity.allowFirstReplicationWhenFrozen);
        HelpBox("If enabled Ascension will allow this entity to perform its first replication even if its frozen.");

        entity.detachOnDisable = EditorGUILayout.Toggle("Detach On Disable", entity.detachOnDisable);
        HelpBox("If enabled this entity will be detached from the network when its disabled.");

        entity.sceneObjectAutoAttach = EditorGUILayout.Toggle("Auto Attach On Load", entity.sceneObjectAutoAttach);
        HelpBox("If enabled this to automatically attach scene entities on map load.");

        entity.autoFreezeProxyFrames = EditorGUILayout.IntField("Auto Freeze Frames", entity.autoFreezeProxyFrames);
        HelpBox("If larger than 0, this entity will be automatically frozen by Ascension for non-owners if it has not received a network update for the amount of frames specified.");

        entity.autoRemoveChildEntities = EditorGUILayout.Toggle("Remove Parent On Detach", entity.autoRemoveChildEntities);
        HelpBox("If enabled this tells Ascension to search the entire transform hierarchy of the entity being detached for nested entities and set their transform.parent to null.");

        entity.clientPredicted = EditorGUILayout.Toggle("Controller Predicted Movement", entity.clientPredicted);
        HelpBox("If enabled this tells Ascension that this entity is using commands for moving and that they are applied on both the owner and controller.");

        EditorGUILayout.LabelField("Scene ID", entity.sceneGuid);
        HelpBox("The scene id of this entity");

        if (settings.clientCanInstantiateAll == false)
        {
            entity.allowInstantiateOnClient = EditorGUILayout.Toggle("Allow Client Instantiate", entity.allowInstantiateOnClient);
            HelpBox("If enabled this prefab can be instantiated by clients, this option can be globally enabled/disabled by changing the 'Instantiate Mode' setting in the 'Window/Ascension/Settings' window");
        }

        if (AscensionNetwork.IsRunning)
        {
            RuntimeInfoGUI(entity);
        }
    }

    public void OnInspectorGUI2()
    {
        //GUILayout.Space(4);

        //GUILayout.BeginHorizontal();
        //GUILayout.Space(2);
        //GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128, 64, 64, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)), Resources.Load("AscensionLogo") as Texture2D);
        //GUILayout.EndHorizontal();

        GUILayout.Space(2);

        EditorGUI.BeginDisabledGroup(Application.isPlaying);

        AscensionEntity entity = (AscensionEntity)target;
        PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

        RuntimeSettings settings = RuntimeSettings.Instance;

#if DEBUG
        EditorGUILayout.LabelField("Prefab Type", prefabType.ToString());
#endif

        // Prefab Id
        switch (prefabType)
        {
            case PrefabType.Prefab:
            case PrefabType.PrefabInstance:
                EditorGUILayout.LabelField("Prefab Id", entity.PrefabId.ToString());

                if (entity.prefabId < 0)
                {
                    EditorGUILayout.HelpBox("Prefab id not set, run the 'Ascension/Emit Code' menu option to correct", MessageType.Error);
                }

                if (prefabType == PrefabType.Prefab)
                {
                    if (PrefabDatabase.Contains(entity) == false)
                    {
                        EditorGUILayout.HelpBox("Prefab lookup not valid, run the 'Ascension/Emit Code' menu option to correct", MessageType.Error);
                    }
                }
                break;

            case PrefabType.None:
                if (entity.prefabId != 0)
                {
                    // force 0 prefab id
                    entity.prefabId = 0;

                    // set dirty
                    EditorUtility.SetDirty(this);
                }

                DEditorGUI.Disabled(() =>
                {
                    EditorGUILayout.IntField("Prefab Id", entity.prefabId);
                });

                break;

            case PrefabType.DisconnectedPrefabInstance:
                entity.prefabId = EditorGUILayout.IntField("Prefab Id", entity.prefabId);

                if (entity.prefabId < 0)
                {
                    EditorGUILayout.HelpBox("Prefab Id not set", MessageType.Error);
                }
                break;
        }

        EditState(entity);
        EditProperties(entity);
        EditSceneProperties(entity, prefabType);

        EditorGUI.EndDisabledGroup();

        if (prefabType == PrefabType.Prefab)
        {
            SaveEntity(entity);
        }
        else
        {
            if (Application.isPlaying)
            {
                RuntimeInfoGUI(entity);
            }
            else
            {
                SaveEntity(entity);
            }
        }
    }

    void DrawPrefabInfo(AscensionEntity entity)
    {
        PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

#if DEBUG
        EditorGUILayout.LabelField("Type", prefabType.ToString());
        EditorGUILayout.LabelField("Scene Id", entity.SceneGuid.ToString());
#endif

        switch (prefabType)
        {
            case PrefabType.Prefab:
            case PrefabType.PrefabInstance:
                EditorGUILayout.LabelField("Id", entity.prefabId.ToString());

                if (entity.prefabId < 0)
                {
                    EditorGUILayout.HelpBox("Prefab id not set, run the 'Ascension/Emit Code' menu option to correct", MessageType.Error);
                }

                if (prefabType == PrefabType.Prefab)
                {
                    if (PrefabDatabase.Contains(entity) == false)
                    {
                        EditorGUILayout.HelpBox("Prefab lookup not valid, run the 'Ascension/Emit Code' menu option to correct", MessageType.Error);
                    }
                }
                break;

            case PrefabType.None:
                if (entity.prefabId != 0)
                {
                    // force 0 prefab id
                    entity.prefabId = 0;

                    // set dirty
                    EditorUtility.SetDirty(this);
                }

                DEditorGUI.Disabled(() =>
                {
                    EditorGUILayout.IntField("Prefab Id", entity.prefabId);
                });

                break;

            case PrefabType.DisconnectedPrefabInstance:
                entity.prefabId = EditorGUILayout.IntField("Prefab Id", entity.prefabId);

                if (entity.prefabId < 0)
                {
                    EditorGUILayout.HelpBox("Prefab Id not set", MessageType.Error);
                }
                break;
        }
    }

    void EditState(AscensionEntity entity)
    {
        RuntimeSettings settings = RuntimeSettings.Instance;

        int selectedIndex;

        selectedIndex = Math.Max(0, Array.IndexOf(serializerIds, entity.SerializerGuid) + 1);
        selectedIndex = EditorGUILayout.Popup("State", selectedIndex, serializerNames);

        if (selectedIndex == 0)
        {
            entity.SerializerGuid = UniqueId.None;
            EditorGUILayout.HelpBox("You must assign a state to this prefab before using it", MessageType.Error);
        }
        else
        {
            entity.SerializerGuid = serializerIds[selectedIndex - 1];
        }

    }


    void EditProperties(AscensionEntity entity)
    {
        RuntimeSettings settings = RuntimeSettings.Instance;

        // Update Rate
        entity.updateRate = EditorGUILayout.IntField("Update Rate", entity.updateRate);
        entity.persistThroughSceneLoads = EditorGUILayout.Toggle("Persist Through Load", entity.persistThroughSceneLoads);
        entity.alwaysProxy = EditorGUILayout.Toggle("Always Proxy", entity.alwaysProxy);
        entity.detachOnDisable = EditorGUILayout.Toggle("Detach On Disable", entity.detachOnDisable);
        entity.allowFirstReplicationWhenFrozen = EditorGUILayout.Toggle("Allow Replication When Frozen", entity.allowFirstReplicationWhenFrozen);

        entity.autoFreezeProxyFrames = EditorGUILayout.IntField("Auto Freeze Frames", entity.autoFreezeProxyFrames);

        entity.clientPredicted = EditorGUILayout.Toggle("Controller Prediction", entity.clientPredicted);
        // Bool Settings


        if (settings.clientCanInstantiateAll == false)
        {
            entity.allowInstantiateOnClient = EditorGUILayout.Toggle("Client Can Instantiate", entity.allowInstantiateOnClient);
        }
    }

    void EditSceneProperties(AscensionEntity entity, PrefabType prefabType)
    {
        bool isSceneObject = prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance || prefabType == PrefabType.None;

        GUILayout.Label("Scene Object Settings", EditorStyles.boldLabel);

        entity.sceneObjectAutoAttach = EditorGUILayout.Toggle("Attach On Load", entity.sceneObjectAutoAttach);
        entity.sceneObjectDestroyOnDetach = EditorGUILayout.Toggle("Destroy On Detach", entity.sceneObjectDestroyOnDetach);

        if (isSceneObject)
        {
            if (!Application.isPlaying && (entity.SceneGuid == UniqueId.None))
            {
                // create new scene id
                entity.SceneGuid = UniqueId.New();

                // save shit (force)
                EditorUtility.SetDirty(this);

                // log it
                Debug.Log(string.Format("Generated scene {0} for {1}", entity.sceneGuid, entity.gameObject.name));
            }

            EditorGUILayout.LabelField("Scene Id", entity.sceneGuid.ToString());
        }
    }

    void SaveEntity(AscensionEntity entity)
    {
        if (GUI.changed)
        {
            EditorUtility.SetDirty(entity);
        }
    }

    void RuntimeInfoGUI(AscensionEntity entity)
    {
        AscensionNetworkInternal.DebugDrawer.IsEditor(true);

        GUILayout.Label("Runtime Info", EditorStyles.boldLabel);
        EditorGUILayout.Toggle("Is Attached", entity.IsAttached);

        if (entity.IsAttached)
        {
            EditorGUILayout.Toggle("Is Owner", entity.IsOwner);

            if (entity.Source != null)
            {
                EditorGUILayout.LabelField("Source", entity.Source.RemoteEndPoint.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Source", "Local");
            }

            if (entity.Controller != null)
            {
                EditorGUILayout.LabelField("Controller", entity.Controller.RemoteEndPoint.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Controller", entity.HasControl ? "Local" : "None");
            }

            EditorGUILayout.LabelField("Proxy Count", entity.AEntity.Proxies.Count.ToString());

            GUILayout.Label("Serializer Debug Info", EditorStyles.boldLabel);
            entity.AEntity.Serializer.DebugInfo();
        }
    }
}


