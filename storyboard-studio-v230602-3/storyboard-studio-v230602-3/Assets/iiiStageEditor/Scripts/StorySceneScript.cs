using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySceneScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        autoAddColi();
    }

    private void OnEnable()
    {
        GameObject[] ScenesObject = GameObject.FindGameObjectsWithTag("StoryScene");
        GameObject Scene;
        for (int i = 0; i < ScenesObject.Length; i++)
        {
            Scene = ScenesObject[i];
            if (Scene != gameObject)
            {
                Scene.SetActive(false);
            }
        }
    }

    private void autoAddColi()
    {
        if (gameObject.GetComponent<MeshFilter>() != null &&  gameObject.GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
        foreach (Transform child in transform)
        {

            if (child.gameObject.GetComponent<MeshFilter>() != null &&  child.gameObject.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<MeshCollider>();
            }
        }
    }
}