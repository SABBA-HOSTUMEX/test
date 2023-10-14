using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeNodeEditor;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine.SceneManagement;
using iiiStoryEditor.UI.ViewModels;

using iiiStoryEditor.UI.ViewModels.ViewModels;
using TMPro;

public class StoryBoardNodeEditor : NodeEditor
{
    public RectTransform NodeGraphRect;
    public NodeGraph Graph;
    public Node StartNode;
    public Node currentNode;
    public Node onPlayingNode;

    public bool isplaying = false;
    public string _savePath;

    public float currNodeRunSec = 0;

    public List<OneSceneNode> StorysScenes;

    public GameObject mainCamera;
    public StoryExportViewModel exportvm;
    public StoryboardViewModel sbvm;

    public Camera CurrViewCamera;
    public AudioSource CurrAudiosource;

    public AudioClip _recordClip;
    float[] _recordsamples;
    public bool previewAudio = false;

    public List<GameObject> SceneGos;
    public bool isExporting = false;

    public float StoryTotalTime = 0;
    public StoryTexNode texNodePrefab;
    public ExportVideoViewModel VideoExportVM;

    public int TotalSec = 0;
    public TMP_Text TotalSecLabel;

    public override void StartEditor(NodeGraph graph)
    {
        base.StartEditor(graph);
        Graph = graph;

        Events.OnGraphPointerClickEvent += OnGraphPointerClick;
        Events.OnNodePointerClickEvent += OnNodePointerClick;
        Events.OnConnectionPointerClickEvent += OnNodeConnectionPointerClick;
        Events.OnSocketConnect += OnConnect;

        Graph.SetSize(Vector2.one * 20000);


        GameObject graphParent = Graph.gameObject.transform.parent.gameObject;
        if(graphParent != null)
        {
            NodeGraphRect = graphParent.GetComponent<RectTransform>();
            NodeGraphRect.transform.localScale = new Vector3(1, 1, 1);
        }

        _savePath = Application.dataPath + "/_graphs/graph.json";
        StartNode = Graph.CreateReturn("Nodes/StoryStartNode", new Vector2(-100,0));    //  your prefab path in resources

        StorysScenes = new List<OneSceneNode>();

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera)
        {

            CurrViewCamera = mainCamera.GetComponent<Camera>();
        }

        GetExporter();
    }
    public StoryExportViewModel GetExporter()
    {
        //IRuntimeEditor Editor = IOC.Resolve<IRuntimeEditor>();
        //RuntimeWindow window = Editor.GetWindow(RuntimeWindowType.StoryExport);
        GameObject ExporterGo = GameObject.FindGameObjectWithTag("ffCapture");
        if (ExporterGo != null)
        {
            exportvm = ExporterGo.GetComponent<StoryExportViewModel>();

            if (exportvm != null)
            {
                Debug.Log("exportvm finded");
                return exportvm;
            }
        }
        return null;
    }

    public void StartExportStory(string type)
    {
        GetExporter();
        //exportvm != null &&
        if ( sbvm != null)
        {
            sbvm.IsPlay = true;
        }
        else
        {
            Debug.Log(" sbvm is null");
        }
    }
    public void clickPlayNodes()
    {

        if (!sbvm.IsPlay)
        {
            startPlayNodes();
            //Time.timeScale = 0.2f;
        }
        else
        {
            stopPlayNodes();
        }
        
    }
    public void CountStoryData()
    {

    }
    public void OnPlaymodeStateChanged(bool isplay)
    {
        if (isplay)
        {
            startPlayNodes();
        }
        else
        {
            Debug.Log("OnPlaymodeStateChanged:");
            stopPlayNodes();
        }
    }
    public void setDefaultCamera(bool set)
    {
        
        if (mainCamera != null)
        {
            mainCamera.SetActive(set);
            if (set)
            {
                Debug.Log("setDefaultCamera");
                if (exportvm != null)
                {
                    exportvm.UpdateCamera(mainCamera.GetComponent<Camera>());
                }
                CurrViewCamera = mainCamera.GetComponent<Camera>();
            }
        }
    }

    public void startPlayNodes()
    {
        Debug.Log("startPlayNodes");
        //Time.timeScale = 0.1f;
        ExposeToEditor[] allSceneGos = GameObject.FindObjectsOfType<ExposeToEditor>(true);
        SceneGos = new List<GameObject>();

        if (allSceneGos != null && allSceneGos.Length > 0)
        {
            GameObject go;
            for(int i = 0; i< allSceneGos.Length; i++)
            {
                go = allSceneGos[i].gameObject;
                if(go.transform.parent == null)
                {
                    SceneGos.Add(go);
                    go.SetActive(false);
                }
            }
        }

        if (StartNode != null)
        {
            if (!sbvm.IsPlay)
            {
                sbvm.IsPlay = true;
            }
            playNextNodeOrFinish(StartNode);
            
        }
    }
    public void HideSceneObjsObPlay()
    {
        if(SceneGos != null)
        {

            GameObject go;
            for (int i = 0; i < SceneGos.Count; i++)
            {
                go = SceneGos[i];
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }
    }
    public void stopPlayNodes()
    {

        if (sbvm.IsPlay)
        {
            sbvm.IsPlay = false;
        }
        if (exportvm != null)
        {
            exportvm.onNodePlayStop();
        }

        //ResetNodesOnStop();
        //setRunSec
        //CancelInvoke();
    }

    public void ResetNodesOnStop()
    {

        Node nodeRow = StartNode;
        Node nextRow;

        while (nodeRow != null)
        {
            nextRow = getNextNodeOrFinish(nodeRow);

            OneSceneNode oneSceneNode = nextRow as OneSceneNode;
            if (oneSceneNode != null)
            {
                oneSceneNode.setRunSec(0);
            }
            nodeRow = nextRow;
        }

    }
    public bool playNextNodeOrFinish(Node node)
    {
        SocketOutput next = GetSingleOutput(node);


        CountStoryData();
        HideSceneObjsObPlay();


        if (next != null && next.HasConnection())
        {
            Connection conn = next.connection;
            PlayOneNode(conn.input.OwnerNode, true);

            return true;
        }
        else
        {
            Debug.Log("next.stopPlayNodes");
            stopPlayNodes();
        }
        return false;
    }

    public void onRecordPause()
    {
        if(currentNode != null)
        {
            OneSceneNode oneSceneNode = currentNode as OneSceneNode;
            oneSceneNode.onPlayPause();
        }
    }
    public void onNodePlayFrameFinish(Node node)
    {
        //OnPlayedFrame
        if(VideoExportVM != null)
        {
            VideoExportVM.OnPlayedFrame();
        }
    }
    public Node getNextNodeOrFinish(Node node)
    {
        SocketOutput next = GetSingleOutput(node);

        if (next != null && next.HasConnection())
        {
            Connection conn = next.connection;
            return conn.input.OwnerNode;
        }
        return null;
    }

    public void PlayOneNode(Node node, bool nextplay)
    {

        Debug.Log("PlayOneNode");

        onPlayingNode = node;
        OneSceneNode oneSceneNode = node as OneSceneNode;
        if(oneSceneNode != null)
        {

            Debug.Log("PlayOneNode > setCurrentNode");
            setCurrentNode(onPlayingNode,true);
            //currNodeRunSec = 0;
            //InvokeRepeating("OnPlayingSec", 1.0f, 1.0f);
        }
    }

    public void OnPlayingSec()
    {
        OneSceneNode oneSceneNode = onPlayingNode as OneSceneNode;
        currNodeRunSec += 1;
        float playpc = (float)currNodeRunSec / (float)oneSceneNode.runsec;

        oneSceneNode.SetPlayProcress(playpc);
        if (currNodeRunSec == oneSceneNode.runsec)
        {
            playNextNodeOrFinish(oneSceneNode);
        }
    }

    public SocketOutput GetSingleOutput(Node node)
    {
        if (StartNode != null)
        {
            List<SocketOutput> outs = node.Outputs;

            if (outs.Count > 0)
            {
                return outs[0];
            }
        }
        return null;
    }

    private void OnConnect(SocketInput arg1, SocketOutput arg2)
    {
        Graph.drawer.SetConnectionColor(arg2.connection.connId, Color.green);
        updateTotalSec();
    }

    private void OnGraphPointerClick(PointerEventData eventData)
    {

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                /*
                {
                    var ctx = new ContextMenuBuilder()
                    .Add("graph/load", () => LoadGraph(_savePath))
                    .Add("graph/save", () => SaveGraph(_savePath))
                    .Build();

                    SetContextMenu(ctx);
                    DisplayContextMenu();
                }
                */
                break;
            case PointerEventData.InputButton.Left:
               // CloseContextMenu();
            break;
        }
    }

    public void SaveGraph(string savePath)
    {
        CloseContextMenu();
        Graph.SaveFile(savePath);
    }
    public void SaveGraph()
    {
        CloseContextMenu();
        string Json = Graph.ExportJson();
        StoryboardScene nodeScene = GameObject.FindObjectOfType<StoryboardScene>();

        if (nodeScene != null)
        {
            nodeScene.m_data = Json;
        }

        //Graph.SaveFile(savePath);
    }

    public void LoadGraph(string savePath)
    {
        CloseContextMenu();
        Graph.Clear();
        Graph.LoadFile(savePath);
    }

    private void OnNodePointerClick(Node node, PointerEventData eventData)
    {
        //Debug.Log("OnNodePointerClick");
        setCurrentNode(node);

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var ctx = new ContextMenuBuilder()
            .Add("複製", () => DuplicateNode(node))
            .Add("刪除連結", () => ClearConnections(node))
            .Add("刪除節點", () => DeleteNode(node))
            .Build();

            SetContextMenu(ctx);
            DisplayContextMenu();
        }
    }

    private void OnNodeConnectionPointerClick(string connId, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var ctx = new ContextMenuBuilder()
            .Add("刪除連結", () => DisconnectConnection(connId))
            .Build();
            SetContextMenu(ctx);
            DisplayContextMenu();
        }
    }


    private void DeleteNode(Node node)
    {
        OneSceneNode oneSceneNode = node as OneSceneNode;
        if (oneSceneNode != null)
        {
            oneSceneNode.OnDelete();
        }
        if(oneSceneNode.m_obj != null)
        {
            Destroy(oneSceneNode.m_obj);
        }
        Graph.Delete(node);
        updateTotalSec();
        CloseContextMenu();
    }

    private void DuplicateNode(Node node)
    {
        Graph.Duplicate(node);

        CloseContextMenu();
    }

    private void DisconnectConnection(string line_id)
    {
        Graph.Disconnect(line_id);
        updateTotalSec();
        CloseContextMenu();
    }

    private void ClearConnections(Node node)
    {
        Graph.ClearConnectionsOf(node);
        updateTotalSec();
        CloseContextMenu();
    }

    public void DisableOthers(GameObject obj) 
    {
        GameObject[] Gameobjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for(var gi = 0; gi < Gameobjs.Length; gi++)
        {
            if (Gameobjs[gi] != obj)
            {
                Gameobjs[gi].SetActive(false);
            }
            else
            {

                Gameobjs[gi].SetActive(true);
            }
        }
    }
    

    public void setCurrentNode(Node node,bool playing = false)
    {
        OneSceneNode inNode = node as OneSceneNode;
        currentNode = node;
        for (var i = 0; i < StorysScenes.Count; i++)
        {
            OneSceneNode nodei = StorysScenes[i];
            if(nodei != null && nodei.getObject() != null)
            {
                if(inNode == nodei)
                {
                    nodei.setActive(true, playing);
                }
                else
                {
                    nodei.setActive(false, false) ;
                }

                if(inNode != null && inNode.m_obj != null)
                {
                    Camera cam = inNode.getCurrCamera();
                    UpdateCurrentCamera(cam);
                }
                else
                {
                    setDefaultCamera(true);
                }
            }
        }
        if(inNode != null && inNode.m_obj != null)
        {

            CurrAudiosource = inNode.m_obj.GetComponentInChildren<AudioSource>();
            _recordClip = AudioClip.Create("sample", 16384, 1, 44100, false, false);
            _recordsamples = new float[_recordClip.samples];


        }
        
    }
    public void onOnePlayFrameed()
    {
        /*
        if(CurrAudiosource != null)
        {
            if (!previewAudio)
            {
                _recordsamples = new float[1024];
                CurrAudiosource.GetOutputData(_recordsamples, 0);
                if (_recordClip != null)
                {
                    _recordClip.SetData(_recordsamples, 0);

                }
            }

            if (Input.GetKeyDown("2") && _recordClip != null)
            {
                AudioSource.PlayClipAtPoint(_recordClip, Camera.main.transform.position);

                previewAudio = true;
            }

        }
        */
    }
    private void Update()
    {
        onOnePlayFrameed();

    }
    public void UpdateCurrentCamera(Camera cam)
    {
        CurrViewCamera = cam;
        if (exportvm != null)
        {
            exportvm.UpdateCamera(CurrViewCamera);
        }

    }

    public Node CreateSceneNode(ProjectItem projectItem,GameObject obj, Vector2 point)
    {
        Debug.Log("CreateSceneNode");
        if (obj == null)
        {
            Debug.Log("CreateSceneNode obj null");
            return null;
        }
        if (obj.GetComponent<StoryTexNode>() != null)
        {
            return CreateTexSceneNode(projectItem,  obj,  point);
        }
            if(point != null)
            {
                currentNode = Graph.CreateReturn("Nodes/OneSceneNode", point);    //  your prefab path in resources
        }
        else
        {
            currentNode = Graph.CreateReturn("Nodes/OneSceneNode", new Vector2(100,100));
        }
            
            OneSceneNode node = currentNode as OneSceneNode;
            if (obj != null)
            {
                StorysScenes.Add(node);
                obj.SetActive(false);

            }
            
            node.InitData(projectItem,obj);

            setCurrentNode(currentNode);
            return currentNode;

    }
    public Node CreateTexSceneNode(ProjectItem projectItem, GameObject obj, Vector2 point)
    {

        currentNode = Graph.CreateReturn("Nodes/OneSceneNode_tex2d", point); 
        OneSceneNodeTex2D node = currentNode as OneSceneNodeTex2D;
        if(texNodePrefab != null)
        {
            IProjectAsync m_project = IOC.Resolve<IProjectAsync>();
            StoryTexNode texNodeObj;
            if (obj == null)
            {
                texNodeObj = GameObject.Instantiate(texNodePrefab);
            }
            else
            {
                if(obj.GetComponent<StoryTexNode>() == null)
                {
                    obj.AddComponent<StoryTexNode>();
                }

                texNodeObj = obj.GetComponent<StoryTexNode>();
            }

            if (obj !=null && obj.GetComponent<UnityEngine.Video.VideoPlayer>() != null)
            {


                UnityEngine.Video.VideoPlayer assetPlayer = obj.GetComponent<UnityEngine.Video.VideoPlayer>();

                texNodeObj.iniVideoData(assetPlayer, node);
            }
            else
            {

                Texture2D assetTex = m_project.Utils.FromProjectItem<Texture2D>(projectItem);

                texNodeObj.iniData(assetTex, node);
            }
            StorysScenes.Add(node);
            texNodeObj.gameObject.SetActive(false);
            node.InitData(projectItem, texNodeObj.gameObject);
        }

        setCurrentNode(currentNode);
        return currentNode;

    }

    
    public void addSceneNode(OneSceneNode node) {
        if(node != null)
        {
            StorysScenes.Add(node);
            if(node.getObject() != null)
            {
                node.m_obj.SetActive(false);
            }
        }
    }
    public void ReleaseNode()
    {
        currentNode = null;
    }
    public List<OneSceneNode> GetNodesByStory()
    {
        List<OneSceneNode> storynodes = new List<OneSceneNode>();

        Node nodeRow = StartNode;
        Node nextRow ;

        TotalSec = 0;

        while (nodeRow != null)
        {
            nextRow = getNextNodeOrFinish(nodeRow);

            OneSceneNode oneSceneNode = nextRow as OneSceneNode;
            if(oneSceneNode != null)
            {
                storynodes.Add(oneSceneNode);
            }
            nodeRow = nextRow;
        }


        return storynodes;
    }

    public int updateTotalSec()
    {
        Node nodeRow = StartNode;
        Node nextRow;

        TotalSec = 0;

        while (nodeRow != null)
        {
            nextRow = getNextNodeOrFinish(nodeRow);

            OneSceneNode oneSceneNode = nextRow as OneSceneNode;
            if (oneSceneNode != null)
            {
                TotalSec += oneSceneNode.runsec;
            }
            nodeRow = nextRow;
        }

        if(TotalSecLabel != null)
        {
            TotalSecLabel.text =  TotalSec.ToString() ;
        }
        return TotalSec;
    }

}
