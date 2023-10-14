using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityObject = UnityEngine.Object;
using Battlehub.RTEditor;
using Battlehub.Utils;
using UnityEngine.EventSystems;
using RuntimeNodeEditor;
namespace iiiStoryEditor.UI.ViewModels
{
    [Binding]
    public class StoryboardViewModel : ViewModel
    {

        private Ray m_pointer;
        [Binding]
        public Ray Pointer
        {
            get { return m_pointer; }
            set { m_pointer = value; }
        }

        private Transform m_cameraTransform;
        [Binding]
        public Transform CameraTransform
        {
            get { return m_cameraTransform; }
            set { m_cameraTransform = value; }
        }

        private ProjectItem m_dragItem;
        private GameObject m_dropTarget;
        private HashSet<Transform> m_prefabInstanceTransforms;
        private GameObject m_prefabInstance;
        protected GameObject PrefabInstance
        {
            get { return m_prefabInstance; }
            set { m_prefabInstance = value; }
        }


        private HashSet<Transform> m_nodeTransforms;
        private Node m_nodeInstance;
        protected Node NodeInstance
        {
            get { return m_nodeInstance; }
            set { m_nodeInstance = value; }
        }

        private IProjectAsync m_project;
        public IPlacementModel m_placement;

        public ProjectItem on_assetItem;
        protected override void Start()
        {
            base.Start();
           

            Init();

        }

        private void Init()
        {
            Debug.Log("StoryboardViewModel");
            m_project = IOC.Resolve<IProjectAsync>();
            if (m_project == null)
            { 
                Debug.LogWarning("RTSLDeps.Project is null");
                Destroy(gameObject);
                return;
            }

            m_project.Events.LoadCompleted += OnLoadCompleted;

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_project = IOC.Resolve<IProjectAsync>();
            m_placement = IOC.Resolve<IPlacementModel>();


            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;

        }

        protected override void OnDisable()
        {
            if (m_project != null)
            {
                m_project.Events.LoadCompleted -= OnLoadCompleted;
            }
            m_project = null;
            base.OnDisable();
            m_project = null;
            m_placement = null;
            Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
        }


        protected virtual void OnPlaymodeStateChanged()
        {

            if(NodeEditor != null)
            {
                NodeEditor.OnPlaymodeStateChanged(IsPlay);
            }
        }

        #region Bound UnityEvent Handlers

        public override void OnDelete()
        {
            Editor.Delete(Editor.Selection.gameObjects);
        }

        public override void OnDuplicate()
        {
            Editor.Duplicate(Editor.Selection.gameObjects);
        }
        private GameObject BGObject;

        public RectTransform NodeEditorRect;
        public StoryBoardNodeEditor NodeEditor;

        public bool FirstDragEnter = true;
        public bool isEnter = false;


        [Binding]
        public virtual bool IsPlay
        {
            get { return Editor.IsPlaying; }
            set
            {
                Editor.IsPlaying = value;
            }
        }


        private Plane m_dragPlane;
        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }



        public override async void OnExternalObjectEnter()
        {
            if (!isEnter)
            {
                isEnter = true;
                FirstDragEnter = true;

                base.OnExternalObjectEnter();

                Debug.Log("Storyboard OnExternalObjectEnter");


                object dragObject = ExternalDragObjects.FirstOrDefault();
                if (ProjectItem.IsAssetItem(dragObject))
                {
                    ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];


                    if (m_project.Utils.ToType(assetItem) == typeof(GameObject) || m_project.Utils.ToType(assetItem) == typeof(Texture2D) )
                    {
                        CanDropExternalObjects = true;

                        Editor.IsBusy = true;
                        UnityObject[] objects;
                        try
                        {
                            objects = await m_project.Safe.LoadAsync(new[] { assetItem });


                            on_assetItem = assetItem;

                            /*

                            if (NodeEditor != null && m_nodeInstance == null)
                            {

                                bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                                if (isOverUI && NodeEditorRect != null)
                                {
                                    Vector2 vec2 = RuntimeNodeEditor.Utility.GetCtxMenuPointerPosition(NodeEditorRect);

                                    if (m_project.Utils.ToType(assetItem) == typeof(GameObject))
                                    {
                                        m_nodeInstance = NodeEditor.CreateSceneNode(assetItem, m_prefabInstance, vec2);
                                    }
                                    else if (m_project.Utils.ToType(assetItem) == typeof(Texture2D))
                                    {
                                        m_nodeInstance = NodeEditor.CreateTexSceneNode(assetItem, m_prefabInstance, vec2);

                                    }

                                }
                            }
                            */


                        }
                        finally
                        {
                            Editor.IsBusy = false;
                        }
                        OnAssetItemLoaded(objects);
                        m_dragItem = null;

                    }
                    else if (m_project.Utils.ToType(assetItem) == typeof(Material))
                    {
                        m_dragItem = assetItem;
                    }
                }
                else if (dragObject is IToolCmd)
                {
                    CanDropExternalObjects = true;
                }


            }
            
        }
        protected virtual void OnLoadCompleted(object sender, ProjectEventArgs<(ProjectItem[] LoadedItems, Object[] LoadedObjects)> e)
        {

            Debug.Log("StoryboardViewModel OnLoadCompleted");
            if (!e.HasError && e.Payload.LoadedItems.Length > 0)
            {
                Debug.Log($"{e.Payload.LoadedItems[0].Name} loaded");

                


            }
        }
        public override void OnExternalObjectLeave()
        {
            if (isEnter)
            {
                if (!FirstDragEnter)
                {
                    base.OnExternalObjectLeave();
                    //Debug.Log("Storyboard OnExternalObjectLeave");

                    if (!Editor.IsBusy)
                    {
                        CanDropExternalObjects = false;
                    }

                    if (m_prefabInstance != null)
                    {
                        Destroy(m_prefabInstance);
                        m_prefabInstance = null;
                        m_prefabInstanceTransforms = null;
                    }
                    if (m_nodeInstance != null)
                    {
                        Destroy(m_nodeInstance);
                        m_nodeInstance = null;

                        if (NodeEditor != null)
                        {
                            NodeEditor.ReleaseNode();
                        }
                    }

                    m_dragItem = null;
                    m_dropTarget = null;
                }
                FirstDragEnter = false;


            }


        }

        public override void OnExternalObjectDrag()
        {
            if (isEnter)
            {
                base.OnExternalObjectDrag();

                Vector3 point;

                if (GetPointOnDragPlane(out point))
                {
                    if (m_prefabInstance != null)
                    {
                        //m_prefabInstance.transform.position = new Vector3(0, 0, 0);
                        m_prefabInstance.transform.position = point;

                        RaycastHit hit = Physics.RaycastAll(Pointer).Where(h => !m_prefabInstanceTransforms.Contains(h.transform)).FirstOrDefault();
                        if (hit.transform != null)
                        {
                            //Debug.Log("RaycastAll hit" + hit.transform.gameObject.name);
                            m_prefabInstance.transform.position = hit.point;
                        }
                    }
                }

                if (m_nodeInstance != null)
                {
                    // m_nodeInstance.transform.position = new Vector3(0, 0, 0);
                    if (NodeEditor != null && Camera.main)
                    {
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(NodeEditor.transform.position);

                        Vector2 nodepos = new Vector3( Input.mousePosition.x - (screenPos.x/100)+200,  Input.mousePosition.y - (screenPos.y/100) +50, Input.mousePosition.z - (screenPos.z / 100));


                        m_nodeInstance.transform.position = nodepos;

                    }
                    else if (NodeEditor.transform != null)
                    {
                       // Debug.Log(NodeEditor.transform.position);
                        
                    }

                }


                if (m_dragItem != null)
                {

                    RaycastHit hitInfo;

                    if (Physics.Raycast(Pointer, out hitInfo, float.MaxValue, Editor.CameraLayerSettings.RaycastMask))
                    {
                        MeshRenderer renderer = hitInfo.collider.GetComponentInChildren<MeshRenderer>();
                        SkinnedMeshRenderer sRenderer = hitInfo.collider.GetComponentInChildren<SkinnedMeshRenderer>();

                        if (renderer != null || sRenderer != null)
                        {
                            CanDropExternalObjects = true;
                            m_dropTarget = hitInfo.transform.gameObject;
                        }
                        else
                        {
                            CanDropExternalObjects = false;
                            m_dropTarget = null;
                        }
                    }
                    else
                    {
                        CanDropExternalObjects = false;
                        m_dropTarget = null;
                    }

                }

                
            }
            
        }

        /*
        public bool PointerIsOverUI(Vector2 screenPos)
        {
            var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
        }

        public GameObject UIRaycast(PointerEventData pointerData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results.Count < 1 ? null : results[0].gameObject;
        }
        static PointerEventData ScreenPosToPointerData(Vector2 screenPos) => new(EventSystem.current) { position = screenPos };
        */

        

        
        public override void OnExternalObjectDrop()
        {
            base.OnExternalObjectDrop();

            //Debug.Log("Storyboard OnExternalObjectDrop");
            if (m_prefabInstance != null)
            {
                RecordUndo();
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;

            }

            if (m_nodeInstance != null)
            {
                m_nodeInstance = null;
                if (NodeEditor != null)
                {
                    NodeEditor.ReleaseNode();
                }
            }

            if (m_dropTarget != null)
            {
                LoadAsync();
                m_dropTarget = null;
                m_dragItem = null;
            }

            IToolCmd cmd = Editor.DragDrop.DragObjects.OfType<IToolCmd>().FirstOrDefault();
            if (cmd != null)
            {
                object result = cmd.Run();
                GameObject go = result as GameObject;
                if (go == null)
                {
                    ExposeToEditor exposeToEditor = result as ExposeToEditor;
                    if (exposeToEditor != null)
                    {
                        go = exposeToEditor.gameObject;
                    }
                }

                if (go != null)
                {
                    Vector3 point;
                    if (GetPointOnDragPlane(out point))
                    {
                        RaycastHit hit = Physics.RaycastAll(Pointer).FirstOrDefault();
                        if (hit.transform != null)
                        {
                            point = new Vector3(0,0,0);
                        }

                        ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                        go.transform.position = new Vector3(0, 0, 0);// point + Vector3.up * exposeToEditor.Bounds.extents.y;
                        if (m_placement != null)
                        {

                            IRuntimeSelectionComponent selectionComponent = m_placement.GetSelectionComponent();
                            if (selectionComponent.CanSelect)
                            {
                                bool wasEnabled = Undo.Enabled;
                                Undo.Enabled = false;
                                Selection.activeGameObject = null;
                                Selection.activeGameObject = go;
                                Undo.Enabled = wasEnabled;
                            }
                        }
                    }
                }
            }

            FirstDragEnter = true;
            isEnter = false; 
        }
        #endregion

        #region Methods

        private async void LoadAsync()
        {
            MeshRenderer renderer = m_dropTarget.GetComponentInChildren<MeshRenderer>();
            SkinnedMeshRenderer sRenderer = m_dropTarget.GetComponentInChildren<SkinnedMeshRenderer>();
            Debug.Log("Storyboard LoadAsync");

            if (renderer == null && sRenderer == null)
            {
                return;
            }
            ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];
            Editor.IsBusy = true;
            UnityObject obj;
            try
            {
                obj = (await m_project.Safe.LoadAsync(new[] { assetItem }))[0];
            }
            catch (System.Exception e)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                if (wm != null)
                {
                    wm.MessageBox("Unable to load asset item ", e.Message);
                    //Debug.LogException(e);
                }
                return;
            }
            finally
            {
                Editor.IsBusy = false;
            }

            if (obj is Material)
            {
                if (renderer != null)
                {
                    Undo.BeginRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        materials[i] = (Material)obj;
                    }
                    renderer.sharedMaterials = materials;
                }

                if (sRenderer != null)
                {
                    Undo.BeginRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    Material[] materials = sRenderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        materials[i] = (Material)obj;
                    }
                    sRenderer.sharedMaterials = materials;
                }

                if (renderer != null || sRenderer != null)
                {
                    Undo.BeginRecord();
                }

                if (renderer != null)
                {
                    Undo.EndRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                }

                if (sRenderer != null)
                {
                    Undo.EndRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                }

                if (renderer != null || sRenderer != null)
                {
                    Undo.EndRecord();
                }
            }
        }

        protected virtual void OnAssetItemLoaded(UnityObject[] objects)
        {

            Debug.Log("Storyboard OnAssetItemLoaded");

            GameObject prefab = objects[0] as GameObject;

            if (prefab == null)
            {
                PrefabNodeEditorCreate();
                return;
            }


            //CreateDragPlane();

            bool wasPrefabEnabled = prefab.activeSelf;
            prefab.SetActive(false);

            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                m_prefabInstance = InstantiatePrefab(prefab, point, prefab.GetComponent<Transform>().rotation);
            }
            else
            {
                m_prefabInstance = InstantiatePrefab(prefab, Vector3.zero, prefab.GetComponent<Transform>().rotation);
            }
            //m_prefabInstance = InstantiatePrefab(prefab, Vector3.zero, prefab.GetComponent<Transform>().rotation);



            if(m_prefabInstance != null)
            {
                Editor.AddGameObjectToHierarchy(m_prefabInstance); 

                m_prefabInstanceTransforms = new HashSet<Transform>(m_prefabInstance.GetComponentsInChildren<Transform>(true));

                prefab.SetActive(wasPrefabEnabled);

                ExposeToEditor exposeToEditor = ExposePrefabInstance(m_prefabInstance);
                exposeToEditor.SetName(prefab.name);

                OnActivatePrefabInstance(m_prefabInstance);


                if (!Editor.DragDrop.InProgress)
                {
                    RecordUndo();
                    m_prefabInstance = null;
                    m_prefabInstanceTransforms = null;
                }
            }

            
        }

        protected virtual GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            //Debug.Log("Storyboard InstantiatePrefab");
            return Instantiate(prefab, position, rotation);
        }

        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {

            //Debug.Log("Storyboard ExposePrefabInstance");
            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }
            return exposeToEditor;
        }

        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            PrefabNodeEditorCreate();
            prefabInstance.SetActive(true);
        }

        public void PrefabNodeEditorCreate()
        {
            if (NodeEditor != null )
            {
                bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                if (isOverUI && NodeEditorRect != null && on_assetItem != null)
                {
                    Vector2 vec2 = RuntimeNodeEditor.Utility.GetCtxMenuPointerPosition(NodeEditorRect);

                    if (m_project.Utils.ToType(on_assetItem) == typeof(GameObject))
                    {
                        m_nodeInstance = NodeEditor.CreateSceneNode(on_assetItem, m_prefabInstance, vec2);
                    }
                    else if (m_project.Utils.ToType(on_assetItem) == typeof(Texture2D))
                    {
                        m_nodeInstance = NodeEditor.CreateTexSceneNode(on_assetItem, m_prefabInstance, vec2);

                    }
                    on_assetItem = null;

                }
            }
        }

        protected virtual void RecordUndo()
        {
            ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();

            Undo.BeginRecord();
            Undo.RegisterCreatedObjects(new[] { exposeToEditor });
            if(m_placement != null)
            {

                IRuntimeSelectionComponent selectionComponent = m_placement.GetSelectionComponent();
                if (selectionComponent != null && selectionComponent.CanSelect)
                {
                    Selection.activeGameObject = m_prefabInstance;
                }
            }

            Undo.EndRecord();
        }

        #endregion


        protected void CreateDragPlane()
        {
            m_dragPlane = m_placement.GetDragPlane(CameraTransform);
        }

        protected virtual Plane GetDragPlane(IScenePivot scenePivot, Vector3 up)
        {
            return m_placement.GetDragPlane(up, scenePivot.SecondaryPivot);
        }

        protected virtual bool GetPointOnDragPlane(out Vector3 point)
        {
            return m_placement.GetPointOnDragPlane(m_dragPlane, Pointer, out point);
        }


    }
}


