using System.Collections;
using System.Collections.Generic;
using Battlehub.RTCommon;
using UnityEngine;
using Battlehub.RTEditor;

public class StortEditorExtension : Battlehub.RTEditor.EditorExtension
{
    private IRuntimeConsole m_console;

    protected override void OnEditorExist()
    {
        base.OnEditorExist();

        m_console = IOC.Resolve<IRuntimeConsole>();
        if (m_console != null)
        {
            m_console.BeforeMessageAdded += OnBeforeMessageAdded;
        }

        
    }

    protected override void OnEditorClosed()
    {
        Debug.Log("OnEditorClosed");
        base.OnEditorClosed();
        if (m_console != null)
        {
            m_console.BeforeMessageAdded -= OnBeforeMessageAdded;
        }

        IOC.ClearAll();
        Restart();
    }

    private void OnBeforeMessageAdded(IRuntimeConsole console, ConsoleLogCancelArgs arg)
    {
        if (arg.LogEntry.LogType == LogType.Log)
        {
            arg.Cancel = true;
        }
    }
    public void Restart()
    {
        string[] endings = new string[]{
         "exe", "x86", "x86_64", "app"
     };
        string executablePath = Application.dataPath + "/..";
        foreach (string file in System.IO.Directory.GetFiles(executablePath))
        {
            foreach (string ending in endings)
            {
                if (file.ToLower().EndsWith("." + ending))
                {
                    System.Diagnostics.Process.Start(executablePath + file);
                    Application.Quit();
                    return;
                }
            }

        }
    }
}
