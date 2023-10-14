using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StoryTexNode : MonoBehaviour
{

    public OneSceneNodeTex2D node;
    public float PlaneW = 1;
    public GameObject planeObj;
    public Camera camObj;
    public VideoPlayer m_player;

    // Start is called before the first frame update
    public void iniData(Texture2D tex, OneSceneNodeTex2D _node)
    {


        Debug.Log("Size is " + tex.width + " by " + tex.height);
        int texW = tex.width;
        int texH = tex.height;
        PlaneW = (float)texW / (float)texH;
        Debug.Log("PlaneW:"+ PlaneW);
        node = _node;
        Renderer m_Renderer = planeObj.GetComponent<MeshRenderer>();
        m_Renderer.material.SetTexture("_MainTex", tex);

        planeObj.transform.localScale = new Vector3(PlaneW, 1.0f, 1);
        node.CurrCamera = camObj;
        _node.texScript = this;

    }

    public void iniVideoData( VideoPlayer videoPlayer, OneSceneNodeTex2D _node)
    {
        Debug.Log("iniVideoData");
        Renderer m_Renderer = planeObj.GetComponent<MeshRenderer>();
        if (m_Renderer != null)
        {
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialRenderer = m_Renderer;

            AudioSource audiosource = videoPlayer.gameObject.GetComponent<AudioSource>();
            if (audiosource == null)
            {
                audiosource = videoPlayer.gameObject.AddComponent<AudioSource>();
            }
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audiosource);
        }
        m_player = videoPlayer;
        _node.m_vplayer = m_player;

        int texW = 1920;// tex.width;
        int texH = 1080;// tex.height;
        PlaneW = (float)texW / (float)texH;
        node = _node;

        planeObj.transform.localScale = new Vector3(PlaneW, 1.0f, 1);
        node.CurrCamera = camObj;

    }

    public void updateVideoDataByNode(OneSceneNodeTex2D _node)
    {
        Debug.Log("updateVideoDataByNode");
    }
    public void onSetCamera()
    {

    }
    /*
    // Update is called once per frame
    void Update()
    {
        if (node.gameObject.active && node.CurrCamera != null)
        {
            Camera cam = node.CurrCamera;
            float pos = (cam.nearClipPlane + 0.01f);

            planeObj.transform.position = cam.transform.position + cam.transform.forward * pos;
            planeObj.transform.LookAt(cam.transform);
            planeObj.transform.Rotate(90.0f, 0.0f, 0.0f);

            float h = (Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f) / 10.0f;

            
        }
    }
    */
}
