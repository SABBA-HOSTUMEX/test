using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Battlehub.RTCommon;

public interface IDependency { }

public class StoryMaterialObject : MonoBehaviour, IDependency
{
    public Texture[] o_texs;
    public string m_videoUrl;

    public VideoPlayer videoPlayer;
    // Start is called before the first frame update

    void Awake()
    {
        IOC.Register<IDependency>(this);
    }

    void OnDestroy()
    {
        IOC.Unregister<IDependency>(this);
    }
    void Start()
    {
        
    }
    private void RuntimeStart()
    {
        Debug.Log("Play");
        if (videoPlayer != null)
        {
            Debug.Log("videoPlayer Play");
            videoPlayer.Play();
        }
    }

    private void OnRuntimeDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }


    public void initMovieTexture()
    {
        if(m_videoUrl != null && m_videoUrl != "" )
        {
            if(videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            Renderer renderder = GetComponent<MeshRenderer>();
            if (renderder != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.url = m_videoUrl;
                videoPlayer.isLooping = true;
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
                videoPlayer.targetMaterialRenderer = renderder;
                videoPlayer.Play();
            }
        }
            
    }
}
