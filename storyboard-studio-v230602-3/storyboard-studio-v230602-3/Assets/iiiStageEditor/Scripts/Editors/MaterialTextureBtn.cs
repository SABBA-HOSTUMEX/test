using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;
using System;
using System.Reflection;
using Battlehub.RTEditor;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.Video;
using SFB;



public class MaterialTextureBtn : MonoBehaviour
{
    public Material mat;
    public Texture2D o_texture;
    public TextMeshProUGUI  Label_txt;
    public GameObject ResetBtnGroup;
    public ExposeToEditor Target;
    public StoryMaterialObject storyMatObj;
    // Start is called before the first frame update
    void Start()
    {
        if(ResetBtnGroup != null)
        {
            //ResetBtnGroup.SetActive(false);
        }
    }
    public void setTarget(ExposeToEditor _target, StoryMaterialObject _storyMatObj)
    {
        Target = _target;
        storyMatObj = _storyMatObj;
    }
    public void setMaterial(Material _mat,Texture _o_tex)
    {
        mat = _mat;
        if(mat != null)
        {
            o_texture = _o_tex as Texture2D;

            updateResetBtn();

            string _name = mat.name.Replace("(Instance)", "");
            setLabel("更換貼圖：" + _name);

        }
        if(storyMatObj != null && storyMatObj.m_videoUrl != null)
        {
            updateMovieTexture();
        }
    }
    public void setLabel(string label)
    {
        if(Label_txt != null)
        {

            Label_txt.text = label;
        }
    }
    public void ResetDefaultTex()
    {

        if(o_texture != null)
        {
            mat.mainTexture = o_texture;
            updateResetBtn();
        }

    }

    public void setTexture()
    {
        if(mat !=null)
        {
            ISelectObjectDialog objectSelector = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SelectObject.ToString(), "Select Texture",
                 (sender, args) =>
                 {
                     if (!objectSelector.IsNoneSelected)
                     {
                           OnTextureSelected((Texture2D)objectSelector.SelectedObject);
                     }
                 });
            objectSelector = IOC.Resolve<ISelectObjectDialog>();
            objectSelector.ObjectType = typeof(Texture2D);

            updateResetBtn();
        }
    }

    private void OnTextureSelected(Texture2D texture)
    {

        mat.mainTexture = texture;
        updateResetBtn();

    }
    public void updateResetBtn()
    {
        if (o_texture != mat.mainTexture)
        {
            ResetBtnGroup.SetActive(true);
        }
        else
        {
            ResetBtnGroup.SetActive(false);
        }
    }

    public void SelectMovieTex()
    {
        if (Target != null && storyMatObj != null)
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            string fileName = "";
            string[] path = StandaloneFileBrowser.OpenFilePanel("選擇影片檔案", "", "mp4", false);

            if (path != null && path.Length > 0 && path[0] != null)
            {
                storyMatObj.m_videoUrl = path[0];
                updateMovieTexture();

            }
            editor.IsBusy = false;

        }
    }
    public void updateMovieTexture()
    {
        if (Target != null && storyMatObj != null)
        {
            storyMatObj.initMovieTexture();
        }
    }
}
