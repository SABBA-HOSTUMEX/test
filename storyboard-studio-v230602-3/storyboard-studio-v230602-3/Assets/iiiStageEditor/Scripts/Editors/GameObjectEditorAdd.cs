using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectEditorAdd : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnSelectedGameObjectInit(GameObject[] selectedObjects)
    {
        if (selectedObjects.Length > 0)
        {
            GameObject gobject;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                gobject = selectedObjects[i];
                if (gobject.GetComponent<StoryCanAnimObject>() == null)
                {
                    gobject.AddComponent<StoryCanAnimObject>();
                }
            }
        }
    }
}
