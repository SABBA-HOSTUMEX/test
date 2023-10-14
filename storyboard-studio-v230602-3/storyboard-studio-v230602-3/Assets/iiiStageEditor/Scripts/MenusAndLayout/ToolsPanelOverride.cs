using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;
using Battlehub.RTEditor;

public class ToolsPanelOverride : EditorExtension
{
    [SerializeField]
    private Transform m_toolsPrefab = null;

    protected override void OnEditorCreated(object obj)
    {
        base.OnEditorCreated(obj);
        //OverrideTools();

        /*
        Battlehub.RTEditor.IRuntimeEditor Editor = IOC.Resolve<Battlehub.RTEditor.IRuntimeEditor>();
        RuntimeWindow ToolWindow = Editor.GetWindow(RuntimeWindowType.ToolsPanel);
        RectTransform rect = ToolWindow.gameObject.GetComponent<RectTransform>();
        if(rect != null)
        {
            rect.offsetMin = new Vector2(0, 1);
            rect.offsetMax = new Vector2(0, 1);
            Debug.Log("!");
        }
        */

    }

    protected override void OnEditorExist()
    {
        base.OnEditorExist();
        /*
        OverrideTools();

        IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
        if (editor.IsOpened)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_toolsPrefab != null)
            {
                wm.SetTools(Instantiate(m_toolsPrefab));
            }
            else
            {
                wm.SetTools(null);
            }
        }
        */
    }

    private void OverrideTools()
    {
        IWindowManager wm = IOC.Resolve<IWindowManager>();
        wm.OverrideTools(m_toolsPrefab);
    }
}
