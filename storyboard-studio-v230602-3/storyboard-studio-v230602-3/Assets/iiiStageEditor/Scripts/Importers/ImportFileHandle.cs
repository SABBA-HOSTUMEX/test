using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTEditor.ViewModels;
public class ImportFileHandle : MonoBehaviour
{

    public ImportFileViewModel importFileVM;
    // Start is called before the first frame update
    void Start()
    {
        if(importFileVM != null)
        {
            Debug.Log("ImportFileHandle");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
