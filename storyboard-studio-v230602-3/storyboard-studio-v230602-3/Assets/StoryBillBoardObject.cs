using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;


public class StoryBillBoardObject : MonoBehaviour
{
    
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if(mainCamera == null)
        {

            IRTE rte = IOC.Resolve<IRTE>();
            mainCamera = rte.ActiveWindow.Camera;
        }

        if (mainCamera != null)
        {
            Vector3 newRotation = mainCamera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            transform.eulerAngles = newRotation;
        }
    }
}