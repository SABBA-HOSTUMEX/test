using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTEditor;
using System.IO;
using UnityGLTF;
using UnityEngine.UI;
using Battlehub.RTCommon;
using SFB;
public class PoseAniEditor : MonoBehaviour
{
    public StoryBoardAppGM GM;

    public bool _runSwitch = false;
    public bool _resetSwitch = false;
    // Start is called before the first frame update

    public Transform AniItemsList;
    public GameObject AniEditItemPrefab;

    //public Dictionary<int, OnAniFrame> AnimationData = new Dictionary<int, OnAniFrame>();

    public List<AniEditorItem> AniTimeline = new List<AniEditorItem>();

    public AniEditorItem CurrItem;
    private RuntimeAnimationClip m_clip;

    public GameObject GoTarget;
    public bool deepChilds = false; 
    public StoryCharAnimObject CharTarget;
    private Avatar avatar;
    public Animator animator;

    public GameObject CanRecUi;


    RuntimeAnimationProperty v_property_pos;
    RuntimeAnimationProperty v_property_pos_x;
    RuntimeAnimationProperty v_property_pos_y;
    RuntimeAnimationProperty v_property_pos_z;
    RuntimeAnimationProperty v_property_rot;
    RuntimeAnimationProperty v_property_rot_x;
    RuntimeAnimationProperty v_property_rot_y;
    RuntimeAnimationProperty v_property_rot_z;
    RuntimeAnimationProperty v_property_rot_w;
    RuntimeAnimationProperty v_property_scale;
    RuntimeAnimationProperty v_property_scale_x;
    RuntimeAnimationProperty v_property_scale_y;
    RuntimeAnimationProperty v_property_scale_z;

    private string UiType = "";


    /*
    public GameObject o_GoTarget;
    public GameObject c_GoTarget;
    public StoryCharAnimObject c_CharTarget;
    public Animator c_animator;
    */

    //public HumanPoseHandler m_PoseHandler;


    void Start()
    {
        if(GM == null)
        {
            GameObject GMGO = GameObject.FindGameObjectWithTag("StoryEditorGM");
            GM = GMGO.GetComponent<StoryBoardAppGM>();
            GM.AniEditor = this;
        }

        //InitReadData();

        if (CharTarget != null )
        {
            initWithEditor(CharTarget);
            UiType = "Editor";
        }
        else if(GoTarget != null)
        {

            initWithEditor(GoTarget);
        }
        //makeRuntimeAniClip();

        //Debug.Log(Display.displays[0].ToString());

    }

    public void initWithEditor(GameObject go)
    {
        UiType = "";
        CharTarget = null;
        GoTarget = go;
        gameObject.SetActive(true);
        initAniSet();
    }
    public void initWithEditor(StoryCanAnimObject aniObj)
    {
        UiType = "Inspector";
        CharTarget = null;
        GoTarget = findAniTargetOnParent(aniObj.gameObject);

        gameObject.SetActive(true);
        initAniSet();
    }
    public GameObject findAniTargetOnParent(GameObject go)
    {
        RuntimeAnimation runtimeAnimation = go.GetComponentInParent<RuntimeAnimation>();

        if (runtimeAnimation != null)
        {
            return runtimeAnimation.gameObject;
        }
        return go;
    }

    public void initWithEditor(StoryCharAnimObject aniCharObj)
    {
        UiType = "Inspector";
        if (aniCharObj !=null )
        {
            CharTarget = null;
            GoTarget = aniCharObj.gameObject;// aniCharObj.effParent;
            gameObject.SetActive(true);
            initAniSet();
        }

        /*
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            AnimatorClipInfo clipinfo = clipInfos[0];
            AnimationClip clip = clipinfo.clip;
        }
        */

    }

    public void ClickPlayPause()
    {
        if(GoTarget != null)
        {
            Animation ani = GoTarget.GetComponent<Animation>();

            if(ani != null)
            {
                if (ani.isPlaying)
                {
                    ani.Stop();
                }
                else
                {
                    ani.Play();
                }

            }
        }
    }


    void InitReadData()
    {
        AniTimeline = new List<AniEditorItem>();
        AniEditorItem[] aniEditorItems = GameObject.FindObjectsOfType<AniEditorItem>();

        foreach(AniEditorItem item in aniEditorItems)
        {
            AniTimeline.Add(item);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_runSwitch)
        {
            _runSwitch = false;
            initAniSet();

        }

        if (_resetSwitch)
        {
            _resetSwitch = false;
        }
    }

    public void initAniSet()
    {
        RuntimeAnimation runtimeAnimation = GoTarget.GetComponent<RuntimeAnimation>();
        Animator animator = GoTarget.GetComponent<Animator>();
        Animation animaton = GoTarget.GetComponent<Animation>();

        if (runtimeAnimation == null && animator != null)
        {
            if(CanRecUi != null)
            {
                CanRecUi.SetActive(true);
            }
            else
            {
                CanRecUi.SetActive(false);
            }
        }else if (runtimeAnimation == null && animaton != null)
        {
            if (CanRecUi != null)
            {
                CanRecUi.SetActive(true);
            }
            else
            {
                CanRecUi.SetActive(false);
            }
        }
        else
        {
            CanRecUi.SetActive(false);
        }


        if (CharTarget != null)
        {
            CharBodyIK BodyIK = CharTarget.GetComponent<CharBodyIK>();
            if (BodyIK != null)
            {
                if (BodyIK.EffectorRoot == null)
                {
                    GameObject effGo = new GameObject("動作設定");
                    BodyIK.EffectorRoot = effGo.transform;
                    BodyIK.EffectorRoot.parent = CharTarget.gameObject.transform;
                }
                GoTarget = BodyIK.EffectorRoot.gameObject;
                readIfHavAnimayion();
            }
        }
        resetTimelineEditor();
        if (runtimeAnimation != null)
        {

            List <RuntimeAnimationClip> clips = runtimeAnimation.Clips;
            RuntimeAnimationClip clip;
            if (clips.Count > 0)
            {
                clip = clips[0];
                readRuntimeClipToEditor(clip);
                

            }
        }
    }

    public void resetTimelineEditor()
    {
        Transform anonymous;

        for (int i = 0; i < AniItemsList.childCount; i++) {
            DestroyImmediate(AniItemsList.GetChild(0).gameObject);
        }
        AniTimeline = new List<AniEditorItem>();
    }


    public void readRuntimeClipToEditor(RuntimeAnimationClip clip)
    {
        //get timelines Count
        Dictionary<string, AniEditorItem> secAniDic = new Dictionary<string, AniEditorItem>();

        GameObject aniItemgo;// = GameObject.Instantiate(AniEditItemPrefab);
        AniEditorItem aniItem;// = go.GetComponent<AniEditorItem>();
        RectTransform rect;

        Transform oneTran;

        foreach (RuntimeAnimationProperty property in clip.Properties)
        {
         //  Debug.Log(property.PropertyPath + ":" + property.Children.Count);

            foreach (RuntimeAnimationProperty oneproperty  in property.Children)
            {
                Keyframe[] kfs = oneproperty.Curve.keys;
                foreach (Keyframe k in kfs)
                {

                    //Debug.Log(oneproperty.PropertyName + "::" + k.time + ">" + k.value);

                    if (!secAniDic.ContainsKey(k.time.ToString())){
                        aniItemgo = GameObject.Instantiate(AniEditItemPrefab);
                        aniItem = aniItemgo.GetComponent<AniEditorItem>();
                        rect = aniItemgo.GetComponent<RectTransform>();

                        rect.transform.parent = AniItemsList;
                        rect.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        aniItem.setTransData(GoTarget.transform, UiType);
                        //aniItem.HoldTransforms = newFrameTrans;
                        AniTimeline.Add(aniItem);

                        secAniDic.Add(k.time.ToString(), aniItem);


                        oneTran =  aniItem.PointRoot.transform.Find(oneproperty.PropertyPath);
                        if(oneTran != null)
                        {
                            matchKeyFrameToAniPoint(oneproperty, oneTran,k);
                      //      Debug.Log("Find:: "+ oneTran.name);
                        }
                    }
                    
                    //aniItem = secAniDic[k.time.ToString()];
                }

            } 
        }
        updateViewportSize();
    }

    public void updateViewportSize()
    {
        RectTransform AniItemsListRect = AniItemsList.GetComponent<RectTransform>();
        Vector2 gridsize = AniItemsList.GetComponent<GridLayoutGroup>().cellSize;

        
        var rowopreone = Math.Floor( AniItemsListRect.rect.width / gridsize.x);
        var rows = Mathf.Ceil(AniItemsList.childCount/ (float)rowopreone);
        float newHeight = (rows+2) * (gridsize.y + 10) ;


        AniItemsListRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }
    public void matchKeyFrameToAniPoint(RuntimeAnimationProperty oneproperty, Transform oneTran , Keyframe k)
    {
        //oneTran
        //Debug.Log("matchKeyFrameToAniPoint:"+ oneproperty.PropertyName);

        //Debug.Log(oneproperty.Component);
    }

    public Transform GetTransformByPath(Transform parent, string path)
    {
        Transform transform = parent.Find(path);

        if (transform != null)
        {
            return transform;
        }
        return null;
    }

    public void readIfHavAnimayion()
    {
        Animation ani = GoTarget.GetComponent<Animation>();
        AnimationClip aniClip = GoTarget.GetComponent<AnimationClip>();
       // RuntimeAnimation runani = GoTarget.GetComponent<RuntimeAnimation>();
       // RuntimeAnimationClip runaniClip = GoTarget.GetComponent<RuntimeAnimationClip>();
        if (ani != null && ani.clip != null)
        {
        }
    }

    public void clickNewFrame()
    {
        if (CharTarget != null)
        {

            newCharSet();
        }
        else if (GoTarget != null)
        {
            newGOset();
        }
    }
    public void clickDelOne()
    {
        if (CurrItem != null){
            Destroy(CurrItem.gameObject);
            AniTimeline.Remove(CurrItem);
            cickSaveAni();
        }
    }


    public void cickSaveAni()
    {

        saveAnimationAsTran();

        //saveAnimationAsAnimator();

    }

    public void saveAnimationAsAnimator()
    {
        RuntimeAnimation animation = getTargetRuntimeAni();

        
        List<RuntimeAnimationClip> Clips = new List<RuntimeAnimationClip>();
        m_clip = GenRuntimeAniClip();
        Transform transform;
        bool isFirst = true;
        int onFrame = 0;
        RuntimeAnimationProperty mainproperty;


        RuntimeAnimationProperty property =   new RuntimeAnimationProperty();
        property.Component = animator;
        property.ComponentTypeName = "UnityEngine.Animator, UnityEngine.CoreModule";
        property.ComponentDisplayName = "Animator";
        property.PropertyDisplayName = "Chest";

        property.AnimationPropertyName = "ChestFrontBack";
        property.PropertyName = "Animator.ChestFrontBack";
        //property.PropertyPath = "Animator.ChestFrontBack";

        property.Curve = new AnimationCurve
        {
            keys = new[]{
                        new Keyframe(0,0),
                        new Keyframe(0.5f,1),
                        new Keyframe(1, 1)
                    }
        };

        

        m_clip.Add(property);
        Clips.Add(m_clip);
        animation.SetClips(Clips, 0);

    }
    public void saveAnimationAsTran()
    {
        RuntimeAnimation animation = getTargetRuntimeAni();
        List<RuntimeAnimationClip> Clips = new List<RuntimeAnimationClip>();
        m_clip = GenRuntimeAniClip();


        Transform transform;
        bool isFirst = true;
        RuntimeAnimationProperty mainproperty;
        RuntimeAnimationProperty property;

        transform = GoTarget.transform;


        List<OnAniFrameTrans> transData;
        float framekey = 0;
        AniTimeline.Sort((x, y) => x.frame.CompareTo(y.frame));

        foreach (AniEditorItem item in AniTimeline) //一項一項讀取字串陣列
        {
            transData = item.transData;
            framekey = item.onSec;

            if (isFirst)
            {
                AniItemToPropCreate(transform, transData[0], "");
            }
            else
            {
                AniItemToPropKey(framekey, transData[0]);
            }
            isFirst = false;


        }
        v_property_pos.Children = new List<RuntimeAnimationProperty>  {
                        v_property_pos_x,v_property_pos_y,v_property_pos_z
                    };
        mainproperty = v_property_pos;


        saveAnimationAsTranByTran(GoTarget.transform, m_clip,true, mainproperty);



        m_clip.Add(mainproperty);
        Clips.Add(m_clip);
        animation.SetClips(Clips, 0);
    }
    public OnAniFrameTrans FindMyAniTransInList(Transform tran, List<OnAniFrameTrans> transData)
    {
        OnAniFrameTrans one;
        for (int i = 0; i < transData.Count; i++)
        {
            one = transData[i];
            if (one.transform == tran)
            {
                return one;
            }
        }

        return null;
    }
    public void saveAnimationAsTranByTran(Transform toptransform, RuntimeAnimationClip m_clip, bool deep, RuntimeAnimationProperty aniprop)
    {
        Transform transform;
        bool isFirst = true;

        int lastChildCount = AniTimeline.Count - 1;
        int itemIndex;
        List<OnAniFrameTrans> transData;
        float framekey = 0;
        string path = "" ;
        OnAniFrameTrans oneTrans;
        for (int i = 0; i < toptransform.childCount; i++)
        {
            isFirst = true;
            itemIndex = 0;
            transform = toptransform.GetChild(i);

            if (transform.gameObject.active)
            {
                //Debug.Log("saveAnimationAsTranByTran:"+ transform.gameObject.name);
                foreach (AniEditorItem item in AniTimeline) //一項一項讀取字串陣列
                {
                    transData = item.transData;
                    framekey = item.onSec;


                    oneTrans = FindMyAniTransInList(transform, transData);
                    if (oneTrans != null)
                    {

                        if (isFirst)
                        {
                            path = transform.name;
                            if (aniprop.PropertyPath != "")
                            {
                                path = aniprop.PropertyPath + "/" + path;
                            }
                            AniItemToPropCreate(transform, oneTrans, path);
                        }
                        else
                        {
                            //Debug.Log("AniItemToPropKey:" + framekey);
                            //Debug.Log(oneTrans.transform.name);
                            //Debug.Log(oneTrans.m_lpos);
                            AniItemToPropKey(framekey, oneTrans);
                        }
                        if (itemIndex == lastChildCount)
                        {
                            v_property_pos.Children = new List<RuntimeAnimationProperty>{
                            v_property_pos_x,v_property_pos_y,v_property_pos_z

                        };

                            v_property_rot.Children = new List<RuntimeAnimationProperty>{
                            v_property_rot_x,v_property_rot_y,v_property_rot_z,v_property_rot_w

                        };
                            v_property_scale.Children = new List<RuntimeAnimationProperty>{
                            v_property_scale_x,v_property_scale_y,v_property_scale_z
                        };


                            if (aniprop.PropertyPath == "")
                            {

                            }

                            m_clip.Add(v_property_pos);
                            m_clip.Add(v_property_scale);
                            m_clip.Add(v_property_rot);
                        }
                        itemIndex++;
                        isFirst = false;
                    }
                    


                }
                if (deep && transform.childCount > 0)
                {

                    saveAnimationAsTranByTran(transform, m_clip, true, v_property_pos);
                }
            }


        }
    }

    public RuntimeAnimationProperty AniItemToPropCreate(Transform _transform, OnAniFrameTrans oneFrameTrans,string namepath )
    {


        

        v_property_pos = AnimationUtility.getAnimationPositionProperty(_transform, namepath);
        v_property_pos_x = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_pos, "x", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.x)
                        });
        v_property_pos_y = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_pos, "y", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.y)
                        });
        v_property_pos_z = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_pos, "z", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.z)
                        });
        Quaternion rot = oneFrameTrans.m_lrot;
        Vector3 qv = oneFrameTrans.m_lrot.eulerAngles;
        

        v_property_rot = AnimationUtility.getAnimationRotationProperty(_transform, namepath);
        v_property_rot_x = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_rot, "x", new[]{
                            new Keyframe(0, rot.x)
                        });
        v_property_rot_y = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_rot, "y", new[]{
                            new Keyframe(0, rot.y)
                        });
        v_property_rot_z = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_rot, "z", new[]{
                            new Keyframe(0, rot.z)
                        });


        v_property_rot_w = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_rot, "w", new[]{
                            new Keyframe(0, rot.w)
                        });


        v_property_scale = AnimationUtility.getAnimationScaleProperty(_transform, namepath);
        v_property_scale_x = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_scale, "x", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.x)
                        });
        v_property_scale_y = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_scale, "y", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.y)
                        });
        v_property_scale_z = AnimationUtility.genRuntimeAnimationPropertyRow(v_property_scale, "z", new[]{
                            new Keyframe(0, oneFrameTrans.m_lpos.z)

                        });

        return v_property_pos;

    }
    public void AniItemToPropKey(float framekey, OnAniFrameTrans oneFrameTrans)
    {

        //Debug.Log("AniItemToPropKey:"+ oneFrameTrans.transform.name + ":framekey");
        //Debug.Log(oneFrameTrans.m_pos);
        v_property_pos_x.Curve.AddKey(framekey,oneFrameTrans.m_lpos.x);
        v_property_pos_y.Curve.AddKey(framekey, oneFrameTrans.m_lpos.y);
        v_property_pos_z.Curve.AddKey(framekey, oneFrameTrans.m_lpos.z);

        Vector3 qv = oneFrameTrans.m_lrot.eulerAngles;
        Quaternion rot = oneFrameTrans.m_lrot;

        v_property_rot_x.Curve.AddKey(framekey, rot.x);
        v_property_rot_y.Curve.AddKey(framekey, rot.y);
        v_property_rot_z.Curve.AddKey(framekey, rot.z);
        v_property_rot_w.Curve.AddKey(framekey, rot.w);



        v_property_scale_x.Curve.AddKey(framekey, oneFrameTrans.m_scale.x);
        v_property_scale_y.Curve.AddKey(framekey, oneFrameTrans.m_scale.y);
        v_property_scale_z.Curve.AddKey(framekey, oneFrameTrans.m_scale.z);
    }


    public RuntimeAnimation getTargetRuntimeAni()
    {

        if (CharTarget != null)
        {
            RuntimeAnimation animation = GoTarget.GetComponent<RuntimeAnimation>();
            if (animation == null)
            {
                animation = GoTarget.gameObject.AddComponent<RuntimeAnimation>();
                animation.Loop = true;
            }
            return animation;
        }
        else
        {
            RuntimeAnimation animation = GoTarget.GetComponent<RuntimeAnimation>();
            if (animation == null)
            {
                animation = GoTarget.AddComponent<RuntimeAnimation>();
                animation.Loop = true;
            }
            return animation;
        }

        
    }

    



    public void loadAniitemToGo(AniEditorItem aniItem)
    {
        updateTransfrsToGo(aniItem);
    }
    public void updateTransfrsToGo(AniEditorItem aniItem)
    {
        Transform transform;

        OnAniFrameTrans _oneFrameTrans;
        OneAniEditPoint edpoint;

        for(int i = 0; i < CurrItem.transData.Count; i++)
        {
            _oneFrameTrans = CurrItem.transData[i];
            updateOneTrans(_oneFrameTrans.transform, _oneFrameTrans);
        }
    }

    public void updateOneTrans(Transform transform, OnAniFrameTrans _oneFrameTrans)
    {
        //Debug.Log("updateOneTrans:"+ transform.name);
        transform.localPosition = _oneFrameTrans.m_lpos;
        transform.localRotation = _oneFrameTrans.m_lrot;
        transform.localScale = _oneFrameTrans.m_scale;
    }
    public void newGOset(float setTime = -1)
    {
        GameObject go = GameObject.Instantiate(AniEditItemPrefab);
        AniEditorItem aniItem = go.GetComponent<AniEditorItem>();
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.transform.parent = AniItemsList;

        if(setTime >= 0)
        {
            aniItem.setFrame(setTime.ToString());
        }

        rect.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        aniItem.setTransData(GoTarget.transform,UiType);
        AniTimeline.Add(aniItem);


        aniItem.SetMeCurrItem();
        updateViewportSize();

    }
    public void newCharSet()
    {
        if(CharTarget != null)
        {
            CharBodyIK BodyIK = CharTarget.GetComponent<CharBodyIK>();
            if(BodyIK != null)
            {
                if(BodyIK.EffectorRoot == null)
                {
                    GameObject effGo = new GameObject("動作設定");
                    BodyIK.EffectorRoot = effGo.transform;
                    BodyIK.EffectorRoot.parent = CharTarget.gameObject.transform;
                }
                GoTarget = BodyIK.EffectorRoot.gameObject;

                GameObject go = GameObject.Instantiate(AniEditItemPrefab);
                RectTransform rect = go.GetComponent<RectTransform>();
                AniEditorItem aniItem = go.GetComponent<AniEditorItem>();
                rect.transform.parent = AniItemsList;
                rect.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                aniItem.setTransData(GoTarget.transform, UiType);

                //aniItem.HoldTransforms = newFrameTrans;
                AniTimeline.Add(aniItem);
                aniItem.SetMeCurrItem();
            }

        }
    }

    public void resetCharSet()
    {

    }
    public void saveAnimation()
    {
        //genAnimationFrame();
    }

    public void SetCurrentEditItem( AniEditorItem newitem)
    {
       

        CurrItem = newitem;

        foreach (AniEditorItem item in AniTimeline) //一項一項讀取字串陣列
        {
            if(item == CurrItem)
            {
                item.SetOnActive();
                loadAniitemToGo(item);
            }
            else
            {
                if(item != null)
                {

                    item.SetOnDisactive();
                }
            }
        }

    }

    public void resetAnimationClip()
    {
        if (m_clip != null && CharTarget != null)
        {
            GameObject go = CharTarget.gameObject;
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            if (animation != null)
            {
                Destroy(animation);
            }
        }


        m_clip = null;
    }
    public RuntimeAnimationClip GenRuntimeAniClip()
    {
        RuntimeAnimationClip clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();
        clip.name = "ani";
        return clip;
    }

    /*
    public void genCharAnimationFrame()
    {
        resetAnimationClip();
        if (m_clip == null && CharTarget != null)
        {
            m_clip = GenJustRuntimeAniClip();

            GameObject go = CharTarget.gameObject;
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            if (animation == null)
            {
                animation = go.gameObject.AddComponent<RuntimeAnimation>();
            }

            Transform transform;
            RuntimeAnimationProperty property;
            int Fr = 60;
            for (int i = 0; i < GM.Bodybones.Length; i++)
            {
                transform = animator.GetBoneTransform(GM.Bodybones[i]);

                //property = getAnimationRotationProperty(transform);

                property = new RuntimeAnimationProperty();
                property.Component = transform;
                property.ComponentTypeName = "UnityEngine.Animator, UnityEngine.CoreModule";
                property.ComponentDisplayName = "Transform";
                property.AnimationPropertyName = "m_LocalPosition";
                property.PropertyDisplayName = "位置";
                property.PropertyName = "Animator.ChLocalPosition";


                property.Children = new List<RuntimeAnimationProperty>
                {
                    genRuntimeAnimationPropertyRow(property,"x",new[]{
                        new Keyframe(0,go.transform.localRotation.x),
                        new Keyframe(Fr, go.transform.localRotation.x)
                    }),

                    genRuntimeAnimationPropertyRow(property,"y",new[]{
                        new Keyframe(0,go.transform.localRotation.y),
                        new Keyframe(Fr, go.transform.localRotation.y)
                    }),

                    genRuntimeAnimationPropertyRow(property,"z",new[]{
                        new Keyframe(0, transform.localRotation.z),
                        new Keyframe(Fr, transform.localRotation.z)
                    })
                };


                m_clip.Add(property);
            }


            animation.Loop = true;
            List<RuntimeAnimationClip> Clips = new List<RuntimeAnimationClip>();
            Clips.Add(m_clip);
            animation.SetClips(Clips, 0);

        }
        //GenJustRuntimeAniClip();

    }

    
    

    public RuntimeAnimationProperty genRuntimeAnimationPropertyRow(RuntimeAnimationProperty property, string pname, Keyframe[] keyframes)
    {


        return new RuntimeAnimationProperty
        {
            Parent = property,
            Component = property.Component,
            ComponentTypeName = property.ComponentTypeName,
            PropertyDisplayName = pname,
            PropertyName = pname,
            Curve = new AnimationCurve
            {
                keys = keyframes
            }

        };
    }
    */


    private bool AniRecting = false;
    private float RecDuration = 0;
    private float AniRecorded = 0;
    private int everyFrame = 1;
    private int RedcoedFrames = 0;
    public void makeRuntimeAniClip( GameObject go)
    {
        GoTarget = go;
        makeRuntimeAniClip();
    }
    public void makeRuntimeAniClip()
    {
        IRTE editor = IOC.Resolve<IRTE>();
        editor.IsBusy = true;
        if (animator == null && GoTarget != null)
        {
            animator = GoTarget.GetComponent<Animator>();
        }
        Animation animation = GoTarget.GetComponent<Animation>();

        if (animator != null)
        {
            RuntimeAnimatorController _runtimeAnimController = animator.runtimeAnimatorController;
            AnimationClip[] clips = _runtimeAnimController.animationClips;
            GameObject poseAniEditorGo = GameObject.FindGameObjectWithTag("AniPoseEditor");
            if (poseAniEditorGo != null)
            {
                if (clips.Length > 0)
                {
                    AnimationClip clip = clips[0];
                    RecDuration = clip.averageDuration;
                    animator.enabled = true;

                    startRecordMakeAni(animator.gameObject, RecDuration);

                }
            }

        }else if (animation != null && animation.GetClipCount() > 0)
        {
            AnimationClip clip = animation.clip;
            RecDuration = clip.averageDuration;
            startRecordMakeAni(GoTarget, RecDuration);
        }
    }



    public void cleanTimelineAnimation()
    {

        RuntimeAnimation runtimeAnimation = GoTarget.GetComponent<RuntimeAnimation>();

        
        if (runtimeAnimation != null)
        {

            runtimeAnimation.Clips = new List<RuntimeAnimationClip>();


        }

        resetTimelineEditor();

    }
    private GameObject onRecGo;
    private float onRecRectime;
    public void startRecordMakeAni(GameObject go, float rectime)
    {

        bool havAniClips = false;
        cleanTimelineAnimation();

        if (AniItemsList.childCount > 0)
        {
            havAniClips = true;
        }

        if (!havAniClips)
        {
            Animator animator = GoTarget.GetComponent<Animator>();
            Animation animation = GoTarget.GetComponent<Animation>();
            bool runRec = false;

            if (animator)
            {

                AnimatorClipInfo[] m_CurrentClipInfos = animator.GetCurrentAnimatorClipInfo(0);
                animator.Play(m_CurrentClipInfos[0].clip.name, 0);

                runRec = true;
            }
            else if (animation != null)
            {

                animation.Play();
                runRec = true;
            }


            if (runRec)
            {
                CharTarget = null;
                initWithEditor(go);
                newGOset(0);
                RecDuration = rectime;
                AniRecorded = 0;
                everyFrame = 10;
                RedcoedFrames = 0;
                AniRecting = true;
            }
            if (CanRecUi != null)
            {
                CanRecUi.SetActive(false);
            }
        }
        else
        {
            onRecGo = go;
            onRecRectime = rectime;
            StartCoroutine(waitToStartRecordMakeAni());
        }
        
    }

    IEnumerator waitToStartRecordMakeAni()
    {
        yield return new WaitForSeconds(0.1f);
        startRecordMakeAni(onRecGo, onRecRectime);
    }
    public void stopRecordMakeAni()
    {

    }
    void FixedUpdate()
    {
        if (AniRecting)
        {

            AniRecorded += Time.deltaTime;
            if (AniRecorded >= RecDuration)
            {
                AniRecting = false;
                //saveAnimationAsAnimator();
                saveAnimationAsTran();
                if (GoTarget.GetComponent<Animator>() != null)
                {
                    GoTarget.GetComponent<Animator>().enabled = false;
                    if (CanRecUi != null)
                    {
                        CanRecUi.SetActive(false);
                    }
                }

                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = false;


                Animation ani = GoTarget.GetComponent<Animation>();

                if (ani != null)
                {
                    ani.Play();
                }

            }
            else
            {
                RedcoedFrames += 1;
                if (RedcoedFrames % everyFrame == 0)
                {
                    newGOset(AniRecorded);
                }
            } 
        }
    }
    public void RuntimeAnimationClips2Gltf()
    {
        if(GoTarget.GetComponent<RuntimeAnimation>() == null)
        {

        }
        else
        {

            GLTFSceneExporter exporter = createGltfGlbExporter();
            ExportToGltf(exporter);
        }

    }
    public async void ExportToGltf(GLTFSceneExporter exporter)
    {
        #if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            string targetPath = "/Users/milkywong/Documents/UnityWorks/storyboard-studio-html/public/data/test.gltf";// " / Users/milkywong/Documents/UnityWorks/storyboard-studio-builds/test.gltf";
        #else
             string targetPath = StandaloneFileBrowser.SaveFilePanel("匯出情境模型", "", "", "gltf"); 
        #endif

        if (targetPath != null && targetPath != "")
        {
            exporter.SaveGLTFandBin(Path.GetDirectoryName(targetPath), Path.GetFileNameWithoutExtension(targetPath));
        }
    }
    public GLTFSceneExporter createGltfGlbExporter()
    {
        if (GoTarget != null)
        {
            Transform[] ExportTransforms = new Transform[1];

            Transform one;

            UnityEngine.Object uo;
            ExportTransforms[0] = GoTarget.transform;

            GLTFSceneExporter exporter = new GLTFSceneExporter(ExportTransforms, new ExportOptions());
            for (int i = 0; i < ExportTransforms.Length; i++)
            {
            }
            return exporter;
        }
        return null;

    }


}
