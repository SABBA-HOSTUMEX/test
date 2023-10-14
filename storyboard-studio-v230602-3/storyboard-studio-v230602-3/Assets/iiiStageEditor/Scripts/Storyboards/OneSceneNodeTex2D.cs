using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using RuntimeNodeEditor;

public class OneSceneNodeTex2D : OneSceneNode
{
    public StoryTexNode texScript;
    public VideoPlayer m_vplayer;
    public int v_onFrame = 0;

    public override void onStartPlay()
    {
        base.onStartPlay();
        v_onFrame = 0;

    }
    public override void onPlayFrame()
    {
        
        if(m_vplayer != null)
        {
            v_onFrame++;
            m_vplayer.frame = v_onFrame;
            m_vplayer.Play();
        }
        onPlayFrameed();
    }

     public override void onPlayPause()
    {
        base.onPlayPause();
        if (m_vplayer != null)
        {
          //  Debug.Log("m_vplayer onPlayPause on :"+ m_vplayer.frame);
            //m_vplayer.Pause();
        }
    }

    public override void OnDeserialize(Serializer serializer)
    {
        base.OnDeserialize(serializer);

        if (m_obj.GetComponent<StoryTexNode>() != null)
        {

            texScript = m_obj.GetComponent<StoryTexNode>();

            if(m_obj.GetComponent<VideoPlayer>() != null)
            {
                texScript.iniVideoData(m_obj.GetComponent<VideoPlayer>(),this);
            }
        }





    }
}
