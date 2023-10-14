using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTHandles;

public class ObjectPositionHandle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPositionBeforeDrag(BaseHandle handle)
    {

    }
    public void OnPositionDrag(BaseHandle handle)
    {
        Debug.Log("OnPositionDrag");
    }
    public void OnPositionDrop(BaseHandle handle)
    {

    }
}
