using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

public class StoryboardToolSet : EditorExtension
{
    [SerializeField]
    private Transform m_toolsPrefab = null;

    protected override void OnEditorCreated(object obj)
    {
        OverrideTools();

        IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();

        editorsMap.AddMapping(typeof(StoryboardScene), 31, true, false);
        editorsMap.AddMapping(typeof(StoryCanAnimObject), 32, true, false);
        editorsMap.AddMapping(typeof(StoryCharAnimObject), 33, true, false);
    }

    protected override void OnEditorExist()
    {
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
    }

    private void OverrideTools()
    {
        IWindowManager wm = IOC.Resolve<IWindowManager>();
        wm.OverrideTools(m_toolsPrefab);
    }
}