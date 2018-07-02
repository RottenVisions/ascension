using Ascension.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

#if UNITY_5
using AC = UnityEditor.Animations.AnimatorController;
using ACP = UnityEngine.AnimatorControllerParameter;
using ACPT = UnityEngine.AnimatorControllerParameterType;
#else
using AC = UnityEditorInternal.AnimatorController;
using ACP = UnityEditorInternal.AnimatorControllerParameter;
using ACPT = UnityEditorInternal.AnimatorControllerParameterType;
#endif

public class AscensionEditorWindow : AscensionWindow
{
    [MenuItem("Ascension/Editor", priority = -99)]
    public static void Open()
    {
        AscensionEditorWindow w;

        w = EditorWindow.GetWindow<AscensionEditorWindow>();
        w.titleContent = new GUIContent("Editor");
        w.name = "Editor";
        w.minSize = new Vector2(300, 400);
        w.Show();
        w.Focus();
    }

    Vector2 scroll;

    new void OnGUI()
    {
        base.OnGUI();

        if (HasProject)
        {
            Editor();
            Header();
        }

        if (GUI.changed)
        {
            Save();
        }

        ClearAllFocus();
    }


    void Editor()
    {
        if (Selected != null)
        {
            GUILayout.BeginArea(new Rect(DEditorGUI.GLOBAL_INSET, 22, position.width - (DEditorGUI.GLOBAL_INSET * 2), position.height - 22));

            scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none);

            GUILayout.Space(5);

            if (Selected is StateDefinition)
            {
                EditState((StateDefinition)Selected);
            }

            if (Selected is ObjectDefinition)
            {
                EditStruct((ObjectDefinition)Selected);
            }

            if (Selected is EventDefinition)
            {
                EditEvent((EventDefinition)Selected);
            }

            if (Selected is CommandDefinition)
            {
                EditCommand((CommandDefinition)Selected);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }

    void Header()
    {
        if (Selected != null)
        {
            EditHeader(Selected);
        }
    }

    RuntimeAnimatorController mecanimController;

    void ImportMecanimLayer(StateDefinition def, AC ac, int layer)
    {
        string name = "MecanimLayer_" + layer + "_Weight";

        PropertyDefinition pdef = def.Properties.FirstOrDefault(x => x.StateAssetSettings.MecanimLayer == layer && x.StateAssetSettings.MecanimMode == MecanimMode.LayerWeight);

        if (pdef == null)
        {
            pdef = CreateProperty(new PropertyStateSettings());
            pdef.PropertyType = new PropertyTypeFloat() { Compression = new FloatCompression { Accuracy = 0.01f, MinValue = 0, MaxValue = 1, Enabled = true } };
            pdef.Name = name;
            pdef.StateAssetSettings.MecanimLayer = layer;
            pdef.StateAssetSettings.MecanimMode = MecanimMode.LayerWeight;
            pdef.StateAssetSettings.MecanimDirection = MecanimDirection.UsingAnimatorMethods;

            Debug.Log(string.Format("Imported Mecanim Layer: {0}", pdef.Name));

            def.Properties.Add(pdef);
        }
        else if (!(pdef.PropertyType is PropertyTypeFloat))
        {
            pdef.PropertyType = new PropertyTypeFloat() { Compression = new FloatCompression { Accuracy = 0.01f, MinValue = 0, MaxValue = 1, Enabled = true } };
            Debug.Log(string.Format("Updated Mecanim Layer: {0}", pdef.Name));
        }
    }

    void ImportMecanimParameter(StateDefinition def, ACP p)
    {
        PropertyType type = null;

        switch (p.type)
        {
            case ACPT.Trigger: type = new PropertyTypeTrigger(); break;
            case ACPT.Bool: type = new PropertyTypeBool(); break;
            case ACPT.Int: type = new PropertyTypeInteger(); break;
            case ACPT.Float: type = new PropertyTypeFloat(); break;
        }

        PropertyDefinition pdef = def.Properties.FirstOrDefault(x => x.Name == p.name);

        if (pdef == null)
        {
            pdef = CreateProperty(new PropertyStateSettings());
            pdef.PropertyType = type;
            pdef.Name = p.name;
            pdef.ReplicationMode = replicationMode;
            pdef.StateAssetSettings.MecanimMode = MecanimMode.Parameter;
            pdef.StateAssetSettings.MecanimDirection = mecanimDirection;

            Debug.Log(string.Format("Imported Mecanim Parameter: {0}", pdef.Name));

            def.Properties.Add(pdef);
        }
        else if (pdef.PropertyType.GetType() != type.GetType())
        {
            pdef.PropertyType = type;
            Debug.Log(string.Format("Updated Mecanim Parameter: {0}", pdef.Name));
        }
    }

    ReplicationMode replicationMode;
    MecanimDirection mecanimDirection;

    void EditState(StateDefinition def)
    {
        DEditorGUI.WithLabel("Inheritance", () =>
        {
            def.IsAbstract = DEditorGUI.ToggleDropdown("Is Abstract", "Is Concrete", def.IsAbstract);
            def.ParentGuid = DEditorGUI.AssetPopup("Parent: ", Project.States.Cast<AssetDefinition>(), def.ParentGuid, Project.GetInheritanceTree(def));
        });

        EditorGUI.BeginDisabledGroup(def.IsAbstract);

        DEditorGUI.WithLabel("Bandwidth", () =>
        {
            GUILayout.BeginHorizontal();
            def.PacketMaxBits = Mathf.Clamp(DEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
            def.PacketMaxProperties = Mathf.Clamp(DEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);
            GUILayout.EndHorizontal();
        });

        EditorGUI.EndDisabledGroup();

        DEditorGUI.WithLabel("Import Mecanim Modes", () => {
            replicationMode = (ReplicationMode)EditorGUILayout.EnumPopup("Replication Mode", replicationMode);
            mecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup("Mecanim Mode", mecanimDirection);
        });

        DEditorGUI.WithLabel("Import Mecanim Parameters", () =>
        {
            mecanimController = EditorGUILayout.ObjectField(mecanimController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;

            if (mecanimController)
            {
                if (GUILayout.Button("Import", EditorStyles.miniButton))
                {
                    try
                    {
                        AC ac = (AC)mecanimController;

#if UNITY_5
            for (int i = 0; i < ac.parameters.Length; ++i) {
              ImportMecanimParameter(def, ac.parameters[i]);
            }
#else
                        for (int i = 0; i < ac.parameterCount; ++i)
                        {
                            ImportMecanimParameter(def, ac.GetParameter(i));
                        }

                        for (int i = 0; i < ac.layerCount; ++i)
                        {
                            ImportMecanimLayer(def, ac, i);
                        }
#endif

                        Save();
                    }
                    finally
                    {
                        mecanimController = null;
                    }
                }
            }
        });

        var groups =
          def.Properties
            .Where(x => x.PropertyType.MecanimApplicable)
            .Where(x => x.StateAssetSettings.MecanimMode != MecanimMode.Disabled)
            .GroupBy(x => x.StateAssetSettings.MecanimDirection);

        if (groups.Count() == 1)
        {
            var currentDirection = groups.First().Key;

            DEditorGUI.WithLabel("Mecanim (State Wide)", () =>
            {
                var selectedDirection = (MecanimDirection)EditorGUILayout.EnumPopup(currentDirection);

                if (currentDirection != selectedDirection)
                {
                    foreach (var property in def.Properties.Where(x => x.PropertyType.MecanimApplicable))
                    {
                        property.StateAssetSettings.MecanimDirection = selectedDirection;
                    }

                    Save();
                }
            });
        }
        else if (groups.Count() > 1)
        {
            DEditorGUI.WithLabel("Mecanim (State Wide)", () =>
            {
                string[] options = new string[] { "Using Animator Methods", "Using Bolt Properties", "Mixed (WARNING)" };

                int index = EditorGUILayout.Popup(2, options);

                if (index != 2)
                {
                    foreach (var property in def.Properties.Where(x => x.PropertyType.MecanimApplicable))
                    {
                        property.StateAssetSettings.MecanimDirection = (MecanimDirection)index;
                    }

                    Save();
                }
            });
        }

        EditPropertyList(def, def.Properties);

        Guid guid = def.ParentGuid;

        while (guid != Guid.Empty)
        {
            var parent = Project.FindState(guid);
            GUILayout.Label(string.Format("Inherited from {0}", parent.Name), DEditorGUI.MiniLabelButtonStyle);

            EditorGUI.BeginDisabledGroup(true);
            EditPropertyList(parent, parent.Properties);
            EditorGUI.EndDisabledGroup();

            guid = parent.ParentGuid;
        }
    }


    void EditStruct(ObjectDefinition def)
    {
        // add button
        EditPropertyList(def, def.Properties);
    }

    void EditEvent(EventDefinition def)
    {
        DEditorGUI.WithLabel("Global Senders", () =>
        {
            def.GlobalSenders = (GlobalEventSenders)EditorGUILayout.EnumPopup(def.GlobalSenders);
            DEditorGUI.SetTooltip("Who can send this as an global event?");
        });

        DEditorGUI.WithLabel("Entity Senders", () =>
        {
            def.EntitySenders = (EntityEventSenders)EditorGUILayout.EnumPopup(def.EntitySenders);
            DEditorGUI.SetTooltip("Who can send this as an entity event?");
        });

        // add button
        EditPropertyList(def, def.Properties);
    }

    PropertyDefinition CreateProperty(PropertyAssetSettings settings)
    {
        PropertyDefinition def = new PropertyDefinition
        {
            Name = "NewProperty",
            PropertyType = new PropertyTypeFloat { Compression = FloatCompression.Default() },
            AssetSettings = settings,
            ReplicationMode = ReplicationMode.Everyone,
        };

        def.Oncreated();
        return def;
    }

    void EditCommand(CommandDefinition def)
    {
        DEditorGUI.WithLabel("Correction Interpolation", () =>
        {
            def.SmoothFrames = DEditorGUI.IntFieldOverlay(def.SmoothFrames, "Frames");
        });

        //DEditorGUI.WithLabel("Compress Zero Values", () => {
        //  def.CompressZeroValues = EditorGUILayout.Toggle(def.CompressZeroValues);
        //});

        // add button
        DEditorGUI.Header("Input", "commands");
        GUILayout.Space(2);
        EditPropertyList(def, def.Input);

        // add button
        DEditorGUI.Header("Result", "position");
        GUILayout.Space(2);
        EditPropertyList(def, def.Result);
    }

    void BeginBackground()
    {
        GUILayout.BeginVertical();
    }

    void EndBackground()
    {
        GUILayout.EndVertical();
    }

    void EditHeader(AssetDefinition def)
    {
        var stateDef = def as StateDefinition;
        var structDef = def as ObjectDefinition;
        var cmdDef = def as CommandDefinition;
        var eventDef = def as EventDefinition;

        GUILayout.BeginArea(new Rect(DEditorGUI.GLOBAL_INSET, DEditorGUI.GLOBAL_INSET, position.width - (DEditorGUI.GLOBAL_INSET * 2), DEditorGUI.HEADER_HEIGHT));
        GUILayout.BeginHorizontal(DEditorGUI.HeaderBackground, GUILayout.Height(DEditorGUI.HEADER_HEIGHT));

        if (def is StateDefinition) { DEditorGUI.IconButton("states"); }
        if (def is ObjectDefinition) { DEditorGUI.IconButton("objects"); }
        if (def is EventDefinition) { DEditorGUI.IconButton("events"); }
        if (def is CommandDefinition) { DEditorGUI.IconButton("commands"); }

        // edit asset name
        GUI.SetNextControlName("AscensionEditorName");
        def.Name = EditorGUILayout.TextField(def.Name);

        if (cmdDef != null)
        {
            if (GUILayout.Button("New Input", EditorStyles.miniButtonLeft, GUILayout.Width(75)))
            {
                cmdDef.Input.Add(CreateProperty(new PropertyCommandSettings()));
                Save();
            }

            if (GUILayout.Button("New Result", EditorStyles.miniButtonRight, GUILayout.Width(75)))
            {
                cmdDef.Result.Add(CreateProperty(new PropertyCommandSettings()));
                Save();
            }
        }
        else
        {
            if (GUILayout.Button("New Property", EditorStyles.miniButton, GUILayout.Width(150)))
            {
                if (stateDef != null)
                {
                    stateDef.Properties.Add(CreateProperty(new PropertyStateSettings()));
                    Save();
                }

                if (structDef != null)
                {
                    structDef.Properties.Add(CreateProperty(new PropertyStateSettings()));
                    Save();
                }

                if (eventDef != null)
                {
                    eventDef.Properties.Add(CreateProperty(new PropertyEventSettings()));
                    Save();
                }
            }
        }

        if (stateDef != null) { ExpandAllOrCollapseAll(stateDef.Properties); }
        if (structDef != null) { ExpandAllOrCollapseAll(structDef.Properties); }
        if (eventDef != null) { ExpandAllOrCollapseAll(eventDef.Properties); }
        if (cmdDef != null) { ExpandAllOrCollapseAll(cmdDef.Input, cmdDef.Result); }

        if (stateDef != null) { Duplicate(stateDef); }
        if (structDef != null) { Duplicate(structDef); }
        if (eventDef != null) { Duplicate(eventDef); }
        if (cmdDef != null) { Duplicate(cmdDef); }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void Duplicate<T>(T obj) where T : AssetDefinition
    {
        if (GUILayout.Button("Duplicate", EditorStyles.miniButton, GUILayout.Width(80)))
        {
            obj = SerializerUtils.DeepClone(obj);
            obj.Guid = Guid.NewGuid();

            Project.RootFolder.Assets = Project.RootFolder.Assets.Add(obj);

            Save();
        }
    }

    void ExpandAllOrCollapseAll(params IEnumerable<PropertyDefinition>[] defs)
    {
        if (defs.SelectMany(x => x).Count(x => x.Expanded) > 0)
        {
            if (GUILayout.Button("Collapse All", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                foreach (var d in defs.SelectMany(x => x))
                {
                    d.Expanded = false;
                }
            }
        }
        else
        {
            if (GUILayout.Button("Expand All", EditorStyles.miniButton, GUILayout.Width(80)))
            {
                foreach (var d in defs.SelectMany(x => x))
                {
                    d.Expanded = true;
                }
            }
        }
    }

    void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            EditProperty(def, list[i], i == 0, i == (list.Count - 1));
        }

        // move nudged property
        for (int i = 0; i < list.Count; ++i)
        {
            switch (list[i].Nudge)
            {
                case -1:
                    if (i > 0)
                    {
                        var a = list[i];
                        var b = list[i - 1];

                        list[i] = b;
                        list[i - 1] = a;
                    }
                    break;

                case +1:
                    if (i + 1 < list.Count)
                    {
                        var a = list[i];
                        var b = list[i + 1];

                        list[i] = b;
                        list[i + 1] = a;
                    }
                    break;
            }
        }

        // remove deleted property
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].Deleted)
            {
                // remove 
                list.RemoveAt(i);

                // rewind index
                i -= 1;

                // save
                Save();
            }
        }

        // adjust properties
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].Adjust != 0)
            {
                var self = list[i];
                var other = list[i + list[i].Adjust];

                list[i + list[i].Adjust] = self;
                list[i] = other;

                self.Adjust = 0;
                other.Adjust = 0;

                Save();
            }
        }

    }

    void EditProperty(AssetDefinition def, PropertyDefinition p, bool first, bool last)
    {
        BeginBackground();

        GUILayout.BeginHorizontal(DEditorGUI.HeaderBackground, GUILayout.Height(DEditorGUI.HEADER_HEIGHT));

        if ((Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control)
        {
            if (DEditorGUI.IconButton("minus-editor"))
            {
                if (EditorUtility.DisplayDialog("Delete Property", string.Format("Do you want to delete '{0}' (Property)?", p.Name), "Yes", "No"))
                {
                    p.Deleted = true;
                }
            }
        }
        else
        {
            if (DEditorGUI.Toggle("arrow-down", "arrow-right", p.Expanded && (p.PropertyType.HasSettings || p.PropertyType.MecanimApplicable)))
            {
                p.Expanded = !p.Expanded;
            }
        }

        if (def is StateDefinition || def is ObjectDefinition)
        {
            p.Name = DEditorGUI.TextFieldOverlay(p.Name, p.Priority.ToString(), GUILayout.Width(181));

            switch (p.ReplicationMode)
            {
                case ReplicationMode.Everyone:
                    DEditorGUI.Toggle("controller-plus", true);
                    break;

                case ReplicationMode.EveryoneExceptController:
                    DEditorGUI.Toggle("controller", false);
                    break;

                case ReplicationMode.OnlyOwnerAndController:
                    DEditorGUI.Toggle("controller-only", true);
                    break;

                case ReplicationMode.LocalForEachPlayer:
                    DEditorGUI.Toggle("owner-only", true);
                    break;
            }

        }
        else
        {
            p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(200));
        }

        DEditorGUI.SetTooltip("Name. The name of this property, has to be a valid C# property name.");

        // edit property type
        DEditorGUI.PropertyTypePopup(def, p);
        DEditorGUI.SetTooltip("Type. The type of this property.");

        EditorGUI.BeginDisabledGroup(def.SortOrder != SortOrder.Manual);

        if (DEditorGUI.IconButton("arrow-down", !last))
        {
            p.Adjust += 1;
        }

        if (DEditorGUI.IconButton("arrow-up", !first))
        {
            p.Adjust -= 1;
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if (p.Controller)
        {
            p.ReplicationMode = ReplicationMode.Everyone;
            p.Controller = false;
            Save();
        }

        if (p.Expanded)
        {
            GUILayout.Space(2);

            //DEditorGUI.WithLabel("Comment", () => {
            //  p.Comment = EditorGUILayout.TextField(p.Comment);
            //});

            if (def is StateDefinition || def is ObjectDefinition)
            {
                DEditorGUI.WithLabel("Replication", () =>
                {
                    p.Priority = DEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);
                    p.ReplicationMode = (ReplicationMode)EditorGUILayout.EnumPopup(p.ReplicationMode);
                });
            }

            if (def is CommandDefinition)
            {
                if (p.PropertyType.CanSmoothCorrections && ((CommandDefinition)def).Result.Contains(p))
                {
                    DEditorGUI.WithLabel("Smooth Corrections", () =>
                    {
                        p.CommandAssetSettings.SmoothCorrection = EditorGUILayout.Toggle(p.CommandAssetSettings.SmoothCorrection);
                    });
                }
            }

            if (p.PropertyType.MecanimApplicable && (def is StateDefinition))
            {
                DEditorGUI.WithLabel("Mecanim", () =>
                {
                    EditorGUILayout.BeginHorizontal();

                    if (p.PropertyType is PropertyTypeFloat)
                    {
                        p.StateAssetSettings.MecanimMode = (MecanimMode)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimMode);
                        EditorGUI.BeginDisabledGroup(p.StateAssetSettings.MecanimMode == MecanimMode.Disabled);

                        p.StateAssetSettings.MecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimDirection);

                        switch (p.StateAssetSettings.MecanimMode)
                        {
                            case MecanimMode.Parameter:
                                if (p.StateAssetSettings.MecanimDirection == MecanimDirection.UsingAscensionProperties)
                                {
                                    p.StateAssetSettings.MecanimDamping = DEditorGUI.FloatFieldOverlay(p.StateAssetSettings.MecanimDamping, "Damping Time");
                                }

                                break;

                            case MecanimMode.LayerWeight:
                                p.StateAssetSettings.MecanimLayer = DEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index");
                                break;
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        p.StateAssetSettings.MecanimMode = (MecanimMode)(int)EditorGUILayout.Popup((int)p.StateAssetSettings.MecanimMode, new string[] { "Disabled", "Parameter" });

                        EditorGUI.BeginDisabledGroup(p.StateAssetSettings.MecanimMode == MecanimMode.Disabled);
                        p.StateAssetSettings.MecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimDirection);

                        if (p.PropertyType is PropertyTypeTrigger)
                        {
                            p.StateAssetSettings.MecanimLayer = DEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index");
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.EndHorizontal();
                });
            }

            if (p.PropertyType.HasSettings)
            {
                PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
            }
        }
        else
        {
            GUILayout.Space(2);
        }

        EditorGUILayout.EndVertical();
    }

    GenericMenu.MenuFunction FilterSetter(PropertyDefinition p, FilterDefinition f)
    {
        return () => { p.Filters ^= f.Bit; };
    }

    void EditFilters(PropertyDefinition p)
    {
        if (Project.UseFilters)
        {
            GUIStyle s = new GUIStyle(EditorStyles.miniButton);
            s.alignment = TextAnchor.MiddleLeft;

            Rect menuRect;

            menuRect = GUILayoutUtility.GetLastRect();
            menuRect.x += 85;

            if (GUILayout.Button("", s, GUILayout.MinWidth(200)))
            {
                GenericMenu menu = new GenericMenu();

                foreach (FilterDefinition f in Project.EnabledFilters)
                {
                    menu.AddItem(new GUIContent(f.Name), f.IsOn(p.Filters), FilterSetter(p, f));
                }

                menu.DropDown(menuRect);
                EditorGUIUtility.ExitGUI();
            }

            // rect of the button
            var r = GUILayoutUtility.GetLastRect();
            var labelRect = r;

            labelRect.xMin += 3;
            labelRect.yMin -= 1;
            labelRect.xMax -= 17;

            //GUILayout.BeginArea(r);

            foreach (FilterDefinition f in Project.EnabledFilters)
            {
                if (f.IsOn(p.Filters))
                {
                    var label = DEditorGUI.MiniLabelWithColor(ToUnityColor(f.Color));
                    var sizex = Mathf.Min(label.CalcSize(new GUIContent(f.Name)).x, labelRect.width);

                    GUI.Label(labelRect, f.Name, label);

                    labelRect.xMin += sizex;
                    labelRect.xMin = Mathf.Min(labelRect.xMin, labelRect.xMax);
                }
            }

            //GUILayout.EndArea();

            GUI.DrawTexture(new Rect(r.xMax - 18, r.yMin, 16, 16), DEditorGUI.LoadIcon("ascen-ico-arrow-down"));
        }
    }

    public static Color ToUnityColor(Color4 c)
    {
        return new Color(c.R, c.G, c.B, c.A);
    }
}
