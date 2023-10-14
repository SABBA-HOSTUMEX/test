using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.RTCommon;

public class CameraCompentEditor : MonoBehaviour
{
    public Battlehub.RTEditor.ComponentEditor editor;
    // Start is called before the first frame update

    private IRTECamera m_rteCamera;
    public RuntimeWindow SceneWindow;
    protected IRTECamera RTECamera
    {
        get { return m_rteCamera; }
    }
    void Start()
    {
        IWindowManager wm = IOC.Resolve<IWindowManager>();
        Transform Sctransform = wm.GetWindow(RuntimeWindowType.Scene.ToString());
        SceneWindow = Sctransform.GetComponent<RuntimeWindow>();

        if(SceneWindow != null)
        {
        }

        if (editor != null)
        {
            SetActiveCamera();
        }


        
    }

    public void SetActiveCamera()
    {
        if (editor != null)
        {
            if (editor.Component.gameObject.GetComponent<ExposeToEditor>() != null)
            {
                ExposeToEditor _obj = editor.Component.gameObject.GetComponent<ExposeToEditor>();
                if(_obj.gameObject.GetComponent<Camera>() != null)
                {
                    Camera.SetupCurrent(_obj.GetComponent<Camera>());
                    if (editor.Component.gameObject.active)
                    {
                        editor.Component.gameObject.SetActive(false);
                    }
                    editor.Component.gameObject.SetActive(true);
                }
            }

            IRTEGraphicsLayer graphicsLayer = SceneWindow.IOCContainer.Resolve<IRTEGraphicsLayer>();
            if (graphicsLayer != null)
            {
                m_rteCamera = graphicsLayer.Camera;
            }

            if (m_rteCamera == null && SceneWindow.Camera != null)
            {
                IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
               

                if (m_rteCamera == null)
                {
                    m_rteCamera = SceneWindow.Camera.gameObject.AddComponent<RTECamera>();
                }

            }

            if (SceneWindow != null)
            {
                /*
                GameObject camgo = SceneWindow.Camera.gameObject;
                GameObject target = editor.Component.gameObject;
                camgo.transform.localPosition = target.transform.position;
                camgo.transform.localRotation = target.transform.rotation;
                */
                //IRuntimeSelection m_selection = IOC.Resolve<IRTE>().Selection;
                //m_selection.activeObject = null;
            }
        }
    }

}