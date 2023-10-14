using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RuntimeNodeEditor;
using UnityEngine.UI;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using Battlehub.RTEditor;

using UnityEngine.Video;
public class OneSceneNode : Node
{
    public enum ScNodeType
    {
        Stage,Minimap
    }
    public ScNodeType type = ScNodeType.Stage;
    public TMP_InputField TimeInput;   //  added from editor
    public SocketOutput outputSocket;   //  added from editor
    public SocketInput inputSocket;     //  added from editor

    public Image display;
    public Texture2D m_previewTexture;
    private Sprite m_previewSprite;
    public GameObject m_obj;
    public bool isActive = false;
    public GameObject ActiveObject;


    public Slider playSlider;

    public TMP_Dropdown TypeSelect;
    public TMP_Dropdown CamsSelect;
    public TMP_Dropdown TransSelect;
    public Camera[] NodeCams;
    public Camera CurrCamera;
    public string linkgoid = "";

    public string m_header;

    public bool playFinishCallNext = false;
    public StoryBoardNodeEditor nodeEditor;

    public int runsec = 5;
    public float runingminsec = 0;

    public int transitset = 0;
    private int camset = 0;

    private Texture2D LastScreen;
    public TransitionObject Transition;

    public override void Setup()
    {
        Register(outputSocket);
        Register(inputSocket);
        SetHeader("float");

        nodeEditor = gameObject.GetComponentInParent<StoryBoardNodeEditor>();
    }

    public void InitData(ProjectItem projectItem, GameObject obj)
    {
       if(obj != null)
        {
            setObj(obj);
            m_header = projectItem.Name;

            SetHeader(m_header);
            m_previewTexture = new Texture2D(130, 130);
            m_previewTexture.LoadImage(projectItem.GetPreview());

            setPreview(m_previewTexture);

            if (TimeInput != null)
            {
                setRunSec(int.Parse(TimeInput.text));
            }
            OnNodeReady();
            
        }
    }

    public void setRunSec(int newVal)
    {
        runsec = newVal;
        if (nodeEditor != null)
        {
            nodeEditor.updateTotalSec();
        }
    }
    public void OnNodeReady()
    {
        //ChildsFix();
        initCamera();
    }
    public void ChildsFix()
    {
        if(m_obj != null)
        {
            VideoPlayer[] vs = m_obj.GetComponentsInChildren<VideoPlayer>();
            VideoPlayer vone;
            AudioSource audiosource;
            if (vs.Length > 0)
            {
                for(int i = 0; i< vs.Length; i++)
                {
                    vone = vs[i];

                    audiosource = vone.gameObject.GetComponent<AudioSource>();
                    if (audiosource == null)
                    {
                        audiosource = vone.gameObject.AddComponent<AudioSource>();
                    }
                    vone.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    vone.SetTargetAudioSource(0, audiosource);
                }
            }

            onCurrFix();



        }
    }
    public void onCurrFix()
    {
        AudioSource[] audiosources = m_obj.GetComponentsInChildren<AudioSource>();
        m_obj.SetActive(true);
        AudioSource audiosource;

        VideoPlayer[] vs = m_obj.GetComponentsInChildren<VideoPlayer>();
        VideoPlayer vone;
        if (vs.Length > 0)
        {
            for (int i = 0; i < vs.Length; i++)
            {
                vone = vs[i];

                vone.Play();
            }
        }
        if (audiosources.Length > 0)
        {
            for (int i = 0; i < audiosources.Length; i++)
            {
                audiosource = audiosources[i];

                audiosource.Play();
            }
        }

        
    }

    
    public void setPreview(Texture2D previewimg)
    {
        m_previewSprite = Sprite.Create(previewimg, new Rect(0, 0, previewimg.width, previewimg.height), new Vector2(0.5f, 0.5f));
        display.sprite = m_previewSprite;
    }
    public void setObj(GameObject obj)
    {

        m_obj = obj;

        if(obj.GetComponent<StoryCharAnimObject>() != null)
        {
            StoryCharAnimObject charAnimObject = obj.GetComponent<StoryCharAnimObject>();
            charAnimObject.node = this;
        }
    }
    public void initCamera()
    {

        if (CamsSelect != null)
        {
            NodeCams = m_obj.GetComponentsInChildren<Camera>(true);


            List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            if (NodeCams == null || NodeCams.Length == 0)
            {
                NodeCams = new Camera[1];
                NodeCams[0] = getCurrCamera();
            }
            for (int ci = 0; ci < NodeCams.Length; ci++)
            {
                option = new TMP_Dropdown.OptionData();
                option.text = NodeCams[ci].name;
                optionDatas.Add(option);
            }
            CamsSelect.AddOptions(optionDatas);

        }

        if (NodeCams != null && NodeCams.Length > 0 )
        {
            for (int ci = 0; ci < NodeCams.Length; ci++)
            {
                if (CurrCamera == null)
                {
                    CurrCamera = NodeCams[ci];
                }
                if (NodeCams[ci] == CurrCamera)
                {
                    NodeCams[ci].gameObject.SetActive(true);
                }
                else
                {
                    NodeCams[ci].gameObject.SetActive(false);

                }
            }
        }
        else
        {
            getCurrCamera();

        }

        CamsSelect.value = camset;
    }
    public void SelectSetCamera( )
    {
        int cameid = CamsSelect.value - 1;
        if (NodeCams != null && NodeCams.Length > cameid && NodeCams[cameid] != null)
        {
            SetCurrentCamera(NodeCams[cameid]);
        }
    }
    public void SelectSetType()
    {
        int typeid = TypeSelect.value ;
        if(typeid == 0)
        {
            type = ScNodeType.Stage;
        }else if (typeid == 1)
        {
            type = ScNodeType.Minimap;
        }
    }

    public void SetCurrentCamera(Camera cam)
    {
        CurrCamera = cam;
        if (m_obj.active)
        {
            UpdatePreviewImage();
        }
        DisableOtherCamers();
    }
    public Camera getCurrCamera()
    {
        if (CurrCamera != null)
        {
            return CurrCamera;
        }
        GameObject cameraGo = new GameObject();
        cameraGo.name = "Camera";
        cameraGo.AddComponent<Camera>();
        cameraGo.transform.parent = m_obj.transform;

        CurrCamera = cameraGo.GetComponent<Camera>();
        CurrCamera.enabled = true;

        DisableOtherCamers();

        return CurrCamera;
    }

    public void DisableOtherCamers()
    {
        if (NodeCams != null && NodeCams.Length > 0)
        {
            for (int ci = 0; ci < NodeCams.Length; ci++)
            {
                if (NodeCams[ci] == CurrCamera)
                {
                    NodeCams[ci].gameObject.SetActive(true);
                }
                else if(NodeCams[ci] != null )
                {
                    NodeCams[ci].gameObject.SetActive(false);

                }
            }
        }

        
    }

    public void onTimeInputChange()
    {
        if (TimeInput != null)
        {

            setRunSec(int.Parse(TimeInput.text));
        }
    }

    public void setActive(bool active,bool playing = false)
    {
        isActive = active;
        if(ActiveObject != null)
        {
            ActiveObject.SetActive(active);

            if (playSlider != null)
            {
                playSlider.gameObject.SetActive(playing);
            }

            
            playAnimationsIfHav(active, playing);
            if (playing)
            {
                StartPlay(playing);
                onCurrFix();

            }
            if(CurrCamera != null)
            {
                CurrCamera.gameObject.SetActive(true);
                CurrCamera.enabled = true;
            }
        }
        if(m_obj != null)
        {
            m_obj.SetActive(active);
        }
    }
    public void playAnimationsIfHav(bool active, bool playing = false)
    {

        Animation anim = m_obj.GetComponent<Animation>();
        if (anim != null)
        {
            anim.playAutomatically = active;
        }

        RuntimeAnimation[] rAnis = m_obj.GetComponentsInChildren<RuntimeAnimation>();
        RuntimeAnimation rAni;

        if (rAnis.Length > 0)
        {
            rAni = rAnis[0];
            rAni.IsPlaying = active;
        }
    }
    public void OnDelete()
    {
        if(m_obj != null)
        {
            Destroy(m_obj);
        }
    }

    public override void OnSerialize(Serializer serializer)
    {
        //  save values on graph save
        serializer.Add("timsec", TimeInput.text);
        serializer.Add("header", m_header);
        serializer.Add("camset", CamsSelect.value.ToString());
        serializer.Add("transet", TransSelect.value.ToString());
        serializer.Add("type", TypeSelect.value.ToString());
        
        byte[] previewTextureBytes = m_previewTexture.EncodeToPNG();
        string previewTextureBase64 = System.Convert.ToBase64String(previewTextureBytes);

        serializer.Add("image", previewTextureBase64 );
        
        string goNodeLabel = m_obj.name;
        if(goNodeLabel.IndexOf("__") <0)
        {
            goNodeLabel = goNodeLabel + "__" + Random.Range(0,10000).ToString();
        }
        m_obj.name = goNodeLabel;
        serializer.Add("gameidcode", goNodeLabel);

        //  it would be good idea to use JsonUtility for complex data
    }

    public override void OnDeserialize(Serializer serializer)
    {
        //  load values on graph load
        var value = serializer.Get("timsec");
        TimeInput.SetTextWithoutNotify(value);


        setRunSec(int.Parse(value));

        m_header = serializer.Get("header");
        SetHeader(m_header);

        linkgoid = serializer.Get("gameidcode");

        if(linkgoid != null)
        {
            GameObject go = findObjectInScene(linkgoid);
            if (go)
            {
                setObj(go);
            }
        }

        string previewTextureBase64 = serializer.Get("image");
        if (previewTextureBase64 != null)
        {
            byte[] previewTextureBytes = System.Convert.FromBase64String(previewTextureBase64);

            m_previewTexture = new Texture2D(130, 130);
            m_previewTexture.LoadImage(previewTextureBytes);
            setPreview(m_previewTexture);

        }

        camset = int.Parse(serializer.Get("camset"));
        transitset = int.Parse(serializer.Get("transet"));
        TransSelect.value = transitset;

        int typeid = int.Parse(serializer.Get("type"));
        TypeSelect.value = typeid;
        if(typeid == 1)
        {
            type = ScNodeType.Minimap;
        }

        if (nodeEditor != null)
        {
            nodeEditor.addSceneNode(this);
        }
        OnNodeReady();
    }

    public void UpdatetransSet()
    {
        transitset = TransSelect.value;
    }
    public GameObject getObject()
    {
        GameObject go = null;
        if (m_obj != null)
        {
            return m_obj;
        }
        if (linkgoid != null)
        {
            go = findObjectInScene(linkgoid);
            if (go)
            {
                setObj(go);
                return m_obj;
            }
        }
        return go;
    }
    public GameObject findObjectInScene(string goid)
    {
        GameObject go = null;
        ExposeToEditor[] snos = GameObject.FindObjectsOfType<ExposeToEditor>(true);

        if (snos.Length > 0)
        {
            for(int i =0; i< snos.Length; i++)
            {
                if(snos[i].gameObject.name  == goid)
                {
                    return snos[i].gameObject;
                }
            }
        }
        return go;

    }
    public void SetPlayProcress(float pc)
    {
        if(playSlider != null)
        {
            playSlider.value = pc;
        }
    }

    public void StartPlay(bool callnext)
    {
        runingminsec = 0; 
        playFinishCallNext = callnext;

        Animation[] animations = m_obj.GetComponentsInChildren<Animation>();
        StoryCharAnimObject[] storyCharAnims = m_obj.GetComponentsInChildren<StoryCharAnimObject>();
        for(int a = 0; a < animations.Length; a++)
        {
            animations[a].playAutomatically = true;
        }
        for (int c = 0; c < storyCharAnims.Length; c++)
        {
          //  storyCharAnims[c].playAniByCode(storyCharAnims[c].ActionCode);
        }

        InvokeRepeating("OnPlayingMinSec", 0.0334f, 0.0334f);

        if (Transition != null && transitset != 0)// && transitset != 0
        {

            Node prevnode = FingPrevNode();
            OneSceneNode prevScnode = prevnode as OneSceneNode;
            if(prevnode != null && prevScnode != null)
            {
                Transition.initData(prevnode, this, getTranSit());
                Transition.startByFMP(1);
            }
            else
            {
                Transition.mode = TransitMode.None;
            }
        }



        onStartPlay();
    }

    public TransitMode getTranSit()
    {

        TransitMode mode = TransitMode.None;
        
        switch (TransSelect.value)
        {
            case 1:
                return TransitMode.Alpha;
            case 2:
                return TransitMode.Deg45;
            case 3:
                return TransitMode.Vertical;
            case 4:
                return TransitMode.Diamond;
            case 5:
                return TransitMode.Textfade;
        }

        return mode;
    }
    public void OnPlayingMinSec()
    {
        runingminsec += 0.0334f;
        onPlayFrame();

        if (Transition.mode != TransitMode.None)
        {
            //Transition.SetFrameByTime(runingminsec);
        }

        float playpc = (float)runingminsec / (float)runsec;

        SetPlayProcress(playpc);

        nodeEditor.onNodePlayFrameFinish(this);

        if (runingminsec > runsec)
        {
            OnPlayingFinish();
        }
    }
    public virtual void onStartPlay()
    {

    }
    public virtual void onPlayFrame()
    {
        onPlayFrameed();
    }
    public virtual void onPlayFrameed()
    {
        nodeEditor.onOnePlayFrameed();
    }
    public virtual void onPlayPause()
    {
        
    }

    public void OnPlayingFinish()
    {

        CancelInvoke();
        if(playFinishCallNext)
        {
            nodeEditor.playNextNodeOrFinish(this);

        }
    }


    Texture2D RTImage(Camera camera)
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();
        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }

    public void UpdatePreviewImage()
    {
        if(CurrCamera != null)
        {
            RenderTexture rt = new RenderTexture(200, 120, 16, RenderTextureFormat.ARGB32);
            CurrCamera.targetTexture = rt;

            m_previewTexture = RTImage(CurrCamera);

            setPreview(m_previewTexture);
            CurrCamera.targetTexture = null;
        }
            
    }

    public Connection GetSingleConnect(SocketInput socketin)
    {
        List<Connection> conns = socketin.Connections;
        if (conns.Count > 0)
        {
            return conns[0];
        }
        return null;
    }

    public SocketOutput GetSingleOutput()
    {
        List<SocketOutput> outs = Outputs;
        if (outs.Count > 0)
        {
            return outs[0];
        }
        return null;
    }


    public SocketInput GetSingleInput()
    {
        List<SocketInput> ins = Inputs;
        if (ins.Count > 0)
        {
            return ins[0];
        }
        return null;
    }
    public Node FingPrevNode()
    {
        SocketInput previn = GetSingleInput();
        if (previn != null && previn.HasConnection())
        {
            Connection conn = GetSingleConnect(previn);
            return conn.output.OwnerNode;
        }
        return null;
    }
}