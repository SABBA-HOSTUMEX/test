using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTCommon;
using TMPro;

public class CharComponentEditor : Battlehub.RTEditor.Views.View
{
    public Dropdown ActionCodeDd;
    public Battlehub.RTEditor.ComponentEditor compeditor;
    private ExposeToEditor Target;

    public StoryCharAnimObject charAniObject;
    public Animator animator;
    public int ActionCode = 0;
    public string[] animdb;

    public TMP_Dropdown ActionDd;


    //public Dictionary<>
    private Battlehub.RTEditor.IRuntimeEditor Editor
    {
        get { return IOC.Resolve<Battlehub.RTEditor.IRuntimeEditor>(); }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (compeditor != null)
        {
            if (compeditor.Component.gameObject.GetComponent<ExposeToEditor>() != null)
            {
                Target = compeditor.Component.gameObject.GetComponent<ExposeToEditor>();
            }
        }
        if (Target != null)
        {
            charAniObject = Target.gameObject.GetComponent<StoryCharAnimObject>();

            if(ActionDd != null)
            {
                ActionDd.SetValueWithoutNotify(charAniObject.ActionCode);
            }
        }
    }

    public void SetActionCode(int actionCode)
    {

        if(charAniObject != null)
        {
            makeRuntimeAniClip();
            charAniObject.playAniByCode(actionCode, animdb[actionCode]);
        }
    }
    private bool needUpdateAnimation = false;
    private float RecDuration = 0;
    private PoseAniEditor poseAniEditor;
    //Record Runtime Animation

    public void makeRuntimeAniClip()
    {

        GameObject poseAniEditorGo = GameObject.FindGameObjectWithTag("AniPoseEditor");
        if (poseAniEditorGo != null)
        {
            poseAniEditor = poseAniEditorGo.GetComponent<PoseAniEditor>();
            poseAniEditor.makeRuntimeAniClip(charAniObject.gameObject);
        }
        /*
        needUpdateAnimation = true;
        animator = charAniObject.gameObject.GetComponent<Animator>();
        if(animator != null)
        {

            RuntimeAnimatorController _runtimeAnimController = animator.runtimeAnimatorController;

                if (_runtimeAnimController != null)
            {
                AnimationClip[] clips = _runtimeAnimController.animationClips;
                GameObject poseAniEditorGo = GameObject.FindGameObjectWithTag("AniPoseEditor");
                if (poseAniEditorGo != null)
                {
                    poseAniEditor = poseAniEditorGo.GetComponent<PoseAniEditor>();
                    if (clips.Length > 0)
                    {
                        AnimationClip clip = clips[0];
                        RecDuration = clip.averageDuration;
                        animator.enabled = true;

                        poseAniEditor.startRecordMakeAni(animator.gameObject, RecDuration);

                    }
                }

            }
            else
            {
                Animation anim =  charAniObject.gameObject.AddComponent<Animation>();

            }


        }
        else
        {
        }
        */
       
    }
    private void StartRecording()
    {

    }
     void FixedUpdate()
     {
        
    }

}
