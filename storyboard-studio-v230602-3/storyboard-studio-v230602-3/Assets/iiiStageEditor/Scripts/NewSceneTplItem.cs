using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using UnityEngine.UI;

public class NewSceneTplItem : MonoBehaviour
{
    public GameObject ScenePrefab;
    public string Name = "";
    public Image Cover;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnSelected()
    {

        var tempColor = Cover.color;
        tempColor.a = 1f;
        Cover.color = tempColor;

    }
    public void OnDisSelected()
    {

        var tempColor = Cover.color;
        tempColor.a = .5f;
        Cover.color = tempColor;
    }
}
