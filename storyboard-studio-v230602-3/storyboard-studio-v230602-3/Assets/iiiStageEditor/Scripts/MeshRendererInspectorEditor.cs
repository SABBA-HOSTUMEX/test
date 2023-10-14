using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;


using Battlehub.UIControls;
using Battlehub.Utils;
using System.Linq;

public class MeshRendererInspectorEditor : MonoBehaviour
{
    public Battlehub.RTEditor.ComponentEditor editor;
    public Transform EditorsPanel;
    private ExposeToEditor Target;
    public MeshRenderer renderer;
    public GameObject SelectTextureBtnPrefab;
    public StoryMaterialObject storyMatObj;
    void Start()
    {
        if (editor != null)
        {
            if (editor.Component.gameObject.GetComponent<ExposeToEditor>() != null)
            {
                Target = editor.Component.gameObject.GetComponent<ExposeToEditor>();
                if (Target.gameObject.GetComponent<MeshRenderer>() != null)
                {
                    renderer = Target.gameObject.GetComponent<MeshRenderer>();
                }
            }
            if(Target != null)
            {
                storyMatObj = Target.gameObject.GetComponent<StoryMaterialObject>();
                if (storyMatObj == null)
                {
                    Target.gameObject.AddComponent<StoryMaterialObject>();
                    storyMatObj = Target.gameObject.GetComponent<StoryMaterialObject>();

                }
            }

            getMeterials();
        }
    }

    public void getMeterials()
    {
        if (renderer != null && SelectTextureBtnPrefab != null)
        {
            Material[] Materials = renderer.materials;
            GameObject btnObj;// = GameObject.Instantiate(SelectTextureBtnPrefab, EditorsPanel);
            MaterialTextureBtn loadTextControl;

            if(storyMatObj.o_texs == null)
            {
                storyMatObj.o_texs = new Texture[Materials.Length];
            }

            for (int i = 0; i < Materials.Length; i++)
            {
                if (i == 0)//liwei edit {
                {
                    //if(Materials[i].mainTexture != null){
                    btnObj = GameObject.Instantiate(SelectTextureBtnPrefab, EditorsPanel);

                    loadTextControl = btnObj.GetComponent<MaterialTextureBtn>();
                    if (loadTextControl != null)
                    {


                        loadTextControl.setTarget(Target, storyMatObj);

                        //if (storyMatObj.o_texs != null && storyMatObj.o_texs.Length > i)
                        if (storyMatObj.o_texs != null && storyMatObj.o_texs.Length > i)
                        {
                            storyMatObj.o_texs[i] = Materials[i].mainTexture;

                            loadTextControl.setMaterial(Materials[i], storyMatObj.o_texs[i]);
                        }
                    }
                    //}

                    Debug.Log(Materials[i].name + ", " + i);
                }// } liwei edit
            }
        }
    }
}
