using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UniVRM10;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine.EventSystems;

public class StoryCharAnimObject : MonoBehaviour
{

    public GameObject EffectorPrefab;
    public FullBodyBipedIK BodyIk;
    public GameObject effParent;
    public string vrmpath;

    public bool CustomAction = false;


    public Animator animator;
    public int ActionCode = 0;
    private int _preActionCode = 0;
    public string AudioLink = "";

    public OneSceneNode node;

    public bool IkEffectoreds = false;

    public List<string> animDb = new List<string>();
    void Start()
    {

        if (BodyIk != null)
        {
            //InitIkEffectors(BodyIk);
        }

        if (gameObject.GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();

        }

        animDb.Add("idle");
        animDb.Add("walking");
        animDb.Add("handrais");
        animDb.Add("shakinghand");
        animDb.Add("talkingphone");
        animDb.Add("clapping");
        animDb.Add("greeting");
        animDb.Add("pushingbtn");
        animDb.Add("openingdoor");
        animDb.Add("talking");

        animDb.Add("Action_Idle");
        animDb.Add("Action_Walk");
        animDb.Add("Action_WaveHand");
        animDb.Add("Action_ShakeHand");
        animDb.Add("Action_Talking");
        animDb.Add("Action_ClapHand");
        animDb.Add("Action_Greeting");
        animDb.Add("Action_TakeButton");
        animDb.Add("Action_OpenDoor");
        animDb.Add("Action_Talking");
        if (ActionCode != 0)
        {
            playAniByCode(ActionCode);
        }

        if (animator != null)
        {
            Avatar avator = animator.avatar;
            if(avator != null)
            {

                if(vrmpath != "")
                {
                    ifVrmRuntimeLoad();
                }
                /*

                Debug.Log("hav Avator");
                Debug.Log(avator.humanDescription);

                animator.avatar = AvatarBuilder.BuildHumanAvatar(gameObject, avator.humanDescription);

                */
                /*
                GameObject StoryBrdGMObj = GameObject.FindGameObjectWithTag("StoryEditorGM");
                StoryBoardAppGM StoryBrdGM = StoryBrdGMObj.GetComponent<StoryBoardAppGM>();

                if (StoryBrdGM != null && StoryBrdGM.animator != null)
                {
                    animator.runtimeAnimatorController = StoryBrdGM.animator.runtimeAnimatorController;
                }

                Vrm10Instance vrm10Instance = gameObject.GetComponent<Vrm10Instance>();

                if(vrm10Instance != null)
                {
                    Debug.Log("vrm10Instance != null");
                    avator = vrm10Instance.Humanoid.CreateAvatar();
                    animator.avatar = avator;

                    vrm10Instance.InitializeAtRuntime(ControlRigGenerationOption.Generate);

                }
                */

                
            }
        }
    }
    public void OnEnable()
    {
        if (ActionCode != 0)
        {
            playAniByCode(ActionCode);
        }
    }
    public async void ifVrmRuntimeLoad()
    {
        Vrm10Instance vrm10Instance = gameObject.GetComponent<Vrm10Instance>();

        if (vrmpath != null && vrmpath  != "" && vrm10Instance == null) {
            /*
            GameObject Root = gameObject;
            
            // humanoid
            var humanoid = Root.AddComponent<UniHumanoid.Humanoid>();
            Vrm10Importer.ModelMap m_map = new Vrm10Importer.ModelMap();
            Avatar m_humanoid = humanoid.CreateAvatar();
            m_humanoid.name = "humanoid";
            animator.avatar = m_humanoid;

            // VrmController
            vrm10Instance = gameObject.AddComponent<Vrm10Instance>();
            vrm10Instance.InitializeAtRuntime(ControlRigGenerationOption.Generate);
            vrm10Instance.enabled = false;
            */

            vrm10Instance = await Vrm10.LoadPathAsync(vrmpath);
            GameObject StoryBrdGMObj = GameObject.FindGameObjectWithTag("StoryEditorGM");
            vrm10Instance.gameObject.AddComponent<ExposeToEditor>();
            StoryCharAnimObject storyCharAnimObject;
            StoryBoardAppGM StoryBrdGM = StoryBrdGMObj.GetComponent<StoryBoardAppGM>();



            if (vrm10Instance != null)
            {
                Animator animator = vrm10Instance.gameObject.GetComponent<Animator>();
                Avatar avatar;
                IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();

                storyCharAnimObject = vrm10Instance.gameObject.AddComponent<StoryCharAnimObject>();

                if (animator != null)
                {
                    
                    storyCharAnimObject.vrmpath = vrmpath;
                    storyCharAnimObject.ActionCode = ActionCode;
                    storyCharAnimObject.AudioLink = AudioLink;

                    if (StoryBrdGM != null && StoryBrdGM.animator != null)
                    {
                        animator.runtimeAnimatorController = StoryBrdGM.animator.runtimeAnimatorController;
                    }

                }

                vrm10Instance.gameObject.transform.parent = gameObject.transform.parent;
                vrm10Instance.gameObject.transform.localPosition = gameObject.transform.localPosition;
                vrm10Instance.gameObject.transform.localRotation = gameObject.transform.localRotation;
                vrm10Instance.gameObject.transform.localScale = gameObject.transform.localScale;


                StoryCanAnimObject oldcanAnimObject = gameObject.GetComponent<StoryCanAnimObject>();
                StoryCanAnimObject vrm10InstancecanAnimObject = vrm10Instance.gameObject.AddComponent<StoryCanAnimObject>();

                vrm10InstancecanAnimObject.audioLink = oldcanAnimObject.audioLink;
                vrm10InstancecanAnimObject.OnSetAniID = oldcanAnimObject.OnSetAniID;
                vrm10InstancecanAnimObject.OnSetAniTime = oldcanAnimObject.OnSetAniTime;


                Animation _Animation = gameObject.GetComponent<Animation>();

                if(_Animation != null)
                {
                    Animation vrm10Animation = vrm10Instance.gameObject.AddComponent<Animation>();
                    vrm10Animation.playAutomatically = true;
                    vrm10Animation.Play();
                    vrm10Animation.clip = _Animation.clip;

                }
                RuntimeAnimation _RuntimeAnimation = gameObject.GetComponent<RuntimeAnimation>();
                if (_RuntimeAnimation != null)
                {
                    RuntimeAnimation vrm10RuntimeAnimation = vrm10Instance.gameObject.AddComponent<RuntimeAnimation>();
                    vrm10RuntimeAnimation.Clips = _RuntimeAnimation.Clips;
                    vrm10RuntimeAnimation.ClipIndex = _RuntimeAnimation.ClipIndex;
                }


                ExposeToEditor[] exposeToEditors = gameObject.GetComponentsInChildren<ExposeToEditor>();
                GameObject ego;
                for(int i = 0;i < exposeToEditors.Length; i++)
                {
                    ego = exposeToEditors[i].gameObject;
                    if(ego.transform.parent == gameObject.transform)
                    {
                        ego.transform.parent = vrm10Instance.gameObject.transform;
                    }
                }
                if(StoryBrdGM != null)
                {
                    storyCharAnimObject.EffectorPrefab = StoryBrdGM.EffectorPrefab;
                }
                if(storyCharAnimObject != null)
                {

                    vrm10Instance.gameObject.SetActive(false);

                    /*
                    GameObject effParent = new GameObject();
                    effParent.name = "動作設定";
                    effParent.AddComponent<ExposeToEditor>();
                    effParent.transform.parent = storyCharAnimObject.gameObject.transform;
                    storyCharAnimObject.effParent = effParent;
                    storyCharAnimObject.transform.parent = vrm10Instance.gameObject.transform;

                    FullBodyBipedIK fullBodyBipedIK = vrm10Instance.gameObject.AddComponent<FullBodyBipedIK>();
                    storyCharAnimObject.BodyIk = fullBodyBipedIK;


                  //  RootMotion.BipedReferences.AutoDetectReferences(ref storyCharAnimObject.BodyIk.references, storyCharAnimObject.BodyIk.transform, new RootMotion.BipedReferences.AutoDetectParams(true, false));


                    storyCharAnimObject.BodyIk.solver.Initiate(storyCharAnimObject.gameObject.transform);


                    

                    // Check for possible errors, if found, do not initiate
                    string message = "";
                    if (fullBodyBipedIK.ReferencesError(ref message))
                    {
                        Debug.Log(message);
                    }

                    // Notify of possible problems, but still initiate
                    if (fullBodyBipedIK.ReferencesWarning(ref message)) Debug.Log(message);

                    // Initiate
                    fullBodyBipedIK.solver.SetToReferences(fullBodyBipedIK.references, fullBodyBipedIK.solver.rootNode);
                    */

                    /*
                    GameObject actionObject = GameObject.Instantiate(StoryBrdGM.HumanActionOPrefab);
                    storyCharAnimObject.BodyIk = actionObject.GetComponent<FullBodyBipedIK>();
                    actionObject.transform.parent = vrm10Instance.gameObject.transform;

                    */
                    //InitIkEffectors(storyCharAnimObject.BodyIk, storyCharAnimObject.effParent);
                    vrm10Instance.gameObject.SetActive(true);

                    EventSystem.current.SetSelectedGameObject(vrm10Instance.gameObject);

                }



                if (node != null)
                {
                    node.setObj(vrm10Instance.gameObject);
                }



                Destroy(gameObject);
            }
        }
    }

    public void playAniByCode(int actionCode,string animateStr)
    {
        ActionCode = actionCode;
        _preActionCode = ActionCode;

        if(animator.runtimeAnimatorController != null)
        {
            animator.Play(animateStr);
        }
        else
        {

            Animation anim = animator.gameObject.GetComponent<Animation>();
            if (anim == null){
                anim = animator.gameObject.AddComponent<Animation>();
            }

            //Debug.LogWarning("animator.runtimeAnimatorController == null", animator);

        }
    }
    public void playAniByCode(int actionCode)
    {
        ActionCode = actionCode;
        _preActionCode = ActionCode;

       //     Debug.Log("playAniByCode:" + actionCode);

        if (animDb != null && actionCode != null && animDb.Count > actionCode )
        {

            animator.Play(animDb[actionCode]);
        }
    }

    public void InitIkEffectors()
    {

        if (BodyIk !=null &&  effParent != null)
        {
            InitIkEffectors(BodyIk, effParent);
        }
    }
    public void InitIkEffectors(FullBodyBipedIK BodyIk)
    {
        if (effParent != null)
        {
            InitIkEffectors(BodyIk, effParent);
        }
    }

    public void InitIkEffectors(FullBodyBipedIK BodyIk, GameObject _effParent)
    {
        if (BodyIk != null)
        {
            //CreateOneIkEffector(effParent.transform, BodyIk.solver.bodyEffector, BodyIk.references.pelvis, "身體");

            if (BodyIk.solver != null && BodyIk.solver.initiated)
            {
                CreateOneIkEffector(_effParent.transform, BodyIk.solver.leftShoulderEffector, BodyIk.references.leftUpperArm, "左肩");

                CreateOneIkChain(_effParent.transform, BodyIk.solver.leftArmChain.bendConstraint, BodyIk.references.leftForearm, "左手腕");
                CreateOneIkEffector(_effParent.transform, BodyIk.solver.leftHandEffector, BodyIk.references.leftHand, "左手");

                CreateOneIkEffector(_effParent.transform, BodyIk.solver.rightShoulderEffector, BodyIk.references.rightUpperArm, "左肩");
                CreateOneIkChain(_effParent.transform, BodyIk.solver.rightArmChain.bendConstraint, BodyIk.references.rightForearm, "右手腕");
                CreateOneIkEffector(_effParent.transform, BodyIk.solver.rightHandEffector, BodyIk.references.rightHand, "右手");


                CreateOneIkEffector(_effParent.transform, BodyIk.solver.leftThighEffector, BodyIk.references.leftThigh, "左腿");
                CreateOneIkChain(_effParent.transform, BodyIk.solver.leftLegChain.bendConstraint, BodyIk.references.leftCalf, "左膝");
                CreateOneIkEffector(_effParent.transform, BodyIk.solver.leftFootEffector, BodyIk.references.leftFoot, "左腳");


                CreateOneIkEffector(_effParent.transform, BodyIk.solver.rightThighEffector, BodyIk.references.rightThigh, "右腿");
                CreateOneIkChain(_effParent.transform, BodyIk.solver.rightLegChain.bendConstraint, BodyIk.references.rightCalf, "右膝");
                CreateOneIkEffector(_effParent.transform, BodyIk.solver.rightFootEffector, BodyIk.references.rightFoot, "右腳");
                IkEffectoreds = true;
            }

        }
        _effParent.SetActive(false);
    }
    public void CreateOneIkChain(Transform parenttransform, IKConstraintBend constraintBend, Transform reference, string _name)
    {
        if (constraintBend != null && constraintBend.bendGoal == null)
        {
            GameObject eff = GameObject.Instantiate(EffectorPrefab);
            eff.name = _name;
            eff.transform.parent = parenttransform;
            eff.transform.position = reference.position;
            eff.transform.rotation = reference.rotation;
            constraintBend.bendGoal = eff.transform;
            constraintBend.weight = .9f;
        }


    }
    public void CreateOneIkEffector(Transform parenttransform, IKEffector effector, Transform reference, string _name)
    {

      //  Debug.Log(effector);
        if (effector.target == null)
        {
            GameObject eff = GameObject.Instantiate(EffectorPrefab);
            eff.name = _name;
            eff.transform.parent = parenttransform;
            eff.transform.position = reference.position;
            eff.transform.rotation = reference.rotation;
            
            effector.target = eff.transform;
            effector.positionWeight = .9f;
            effector.rotationWeight = .9f;
            
        }


    }

    public void SetActionById(int ActionCode)
    {

    }

    private void OnRuntimeEditorOpened()
    {
        //Debug.Log("Editor Opened");
    }

    private void OnRuntimeEditorClosed()
    {
       // Debug.Log("Editor Closed");
    }

    private void RuntimeAwake()
    {
        Debug.Log("Awake in play mode");
        if (ActionCode != null)
        {
            playAniByCode(ActionCode);
        }
    }

    private void RuntimeStart()
    {

        if (ActionCode != null)
        {
            playAniByCode(ActionCode);
        }
        Debug.Log("Start in play mode");
    }

    private void OnRuntimeDestroy()
    {
       // Debug.Log("Destroy in play mode");

        playAniByCode(0);
    }

    private void OnRuntimeActivate()
    {
       // Debug.Log("Game View activated");
    }

    private void OnRuntimeDeactivate()
    {
       // Debug.Log("Game View deactivated");
    }
    void Update()
    {
        if(_preActionCode != ActionCode)
        {
            playAniByCode(ActionCode);
        }
    }


    void LateUpdate()
    {
        if (BodyIk != null)
        {
            if(BodyIk.solver.initiated && !IkEffectoreds)
            {

                //InitIkEffectors();
            }

        }

    }
}
