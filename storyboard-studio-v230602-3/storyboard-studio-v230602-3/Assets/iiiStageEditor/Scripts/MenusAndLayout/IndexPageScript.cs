using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class IndexPageScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GC.Collect();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetSceneToOneEdit()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    public void SetSceneToStoryEdit()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

    public void SetSceneToStreamEdit()
    {
        SceneManager.LoadScene(3, LoadSceneMode.Single);
    }
    public void quitApp()
    {
        Application.Quit();
    }
}
