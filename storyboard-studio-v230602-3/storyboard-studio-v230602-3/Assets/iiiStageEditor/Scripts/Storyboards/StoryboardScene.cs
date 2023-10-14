using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;

public class StoryboardScene : MonoBehaviour
{
    public string m_data = "";

    private IRuntimeEditor Editor
    {
        get { return IOC.Resolve<IRuntimeEditor>(); }
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (m_data != "{}" && m_data != "")
        {
            StartCoroutine(delayLoadJson());
            
        }   

    }
    

    IEnumerator delayLoadJson()
    {
        yield return new WaitForSeconds(1);
        RuntimeWindow window = Editor.GetWindow(RuntimeWindowType.Storyboard);
        StoryBoardNodeEditor NodeEditor = window.GetComponentInChildren<StoryBoardNodeEditor>();
        NodeEditor.Graph.Clear();
        if (NodeEditor != null && NodeEditor.Graph != null)
        {
            NodeEditor.Graph.LoadJson(m_data);

        }
    }

// Update is called once per frame
    void Update()
    {
        
    }
}
