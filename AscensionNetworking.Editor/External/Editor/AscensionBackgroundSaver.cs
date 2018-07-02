using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using Ascension.Compiler;
using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;

[InitializeOnLoad]
static class AscensionBackgroundSaver
{
    static Thread thread;
    static AutoResetEvent saveEvent;
    static Queue<Project> saveQueue;

    static AscensionBackgroundSaver()
    {
        saveEvent = new AutoResetEvent(false);
        saveQueue = new Queue<Project>();

        thread = new Thread(SaveThread);
        thread.IsBackground = true;
        thread.Start();
    }

    static void SaveThread()
    {
        while (true)
        {
            if (saveEvent.WaitOne())
            {
                Project project = null;

                lock (saveQueue)
                {
                    while (saveQueue.Count > 0)
                    {
                        project = saveQueue.Dequeue();
                    }
                }

                if (project != null)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(AscensionWindow.ProjectTempOldPath));
                        Directory.CreateDirectory(Path.GetDirectoryName(AscensionWindow.ProjectTempNewPath));

                        // copy current project
                        if (File.Exists(AscensionWindow.ProjectPath))
                        {
                            File.Copy(AscensionWindow.ProjectPath, AscensionWindow.ProjectTempOldPath, true);
                        }

                        // write new project
                        File.WriteAllBytes(AscensionWindow.ProjectTempNewPath, project.ToByteArray());

                        // copy new project to correct path
                        File.Copy(AscensionWindow.ProjectTempNewPath, AscensionWindow.ProjectPath, true);
                    }
                    catch (Exception exn)
                    {
                        Debug.LogException(exn);
                    }
                }
            }
        }
    }

    static public void Save(Project project)
    {
        if ((thread == null) || (thread.IsAlive == false))
        {
            Debug.LogError("ASCENSION SAVE THREAD NOT RUNNING");
            return;
        }

        lock (saveQueue)
        {
            saveQueue.Enqueue(project.DeepClone());
            saveEvent.Set();
        }
    }
}