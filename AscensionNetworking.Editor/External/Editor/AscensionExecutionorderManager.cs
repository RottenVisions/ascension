using System;
using System.Collections.Generic;
using Ascension.Networking;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AscensionExecutionOrderManager
{
    static AscensionExecutionOrderManager()
    {
        Dictionary<string, MonoScript> monoScripts = new Dictionary<string, MonoScript>();

        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            try
            {
                //object o = monoScript.GetType().GetProperty("editorGraphData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(monoScript, null);
                monoScripts.Add(monoScript.name, monoScript);
            }
            catch { }
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (monoScripts.ContainsKey(type.Name))
                {
                    foreach (AscensionExecutionOrderAttribute attribute in type.GetCustomAttributes(typeof(AscensionExecutionOrderAttribute), false))
                    {
                        if (MonoImporter.GetExecutionOrder(monoScripts[type.Name]) != attribute.ExecutionOrder)
                        {
                            MonoImporter.SetExecutionOrder(monoScripts[type.Name], attribute.ExecutionOrder);
                        }
                    }
                }
            }
        }
    }
}