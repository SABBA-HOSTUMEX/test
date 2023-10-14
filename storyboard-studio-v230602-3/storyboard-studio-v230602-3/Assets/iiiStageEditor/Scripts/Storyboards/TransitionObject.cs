using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeNodeEditor;

using FMPUtils.Visuals.CameraTransition;
public class TransitionObject : MonoBehaviour
{

    
    public OneSceneNode ObjectA;
    public OneSceneNode ObjectB;
    public float currTime =0 ;
    public float totalTime = 1;
    public TransitMode mode = TransitMode.None;


    [SerializeField] private Material transitionTestMaterial;
    [SerializeField] public Texture transitionTestMaskTexture;




    // Start is called before the first frame update
    void Start()
    {
         
    }
    public void initData(Node nodeA, Node nodeB, TransitMode tranmode )
    {
        ObjectA = nodeA as OneSceneNode;
        ObjectB = nodeB as OneSceneNode;

        mode = tranmode;

    }

    public void startByFMP(float Duration)
    {
        bool reassignAudioListenerToTargetCamera = true;
        Camera fromCamera = null;
        Camera toCamera = null;
        if (ObjectA != null && ObjectA.m_obj != null) {
            fromCamera = ObjectA.CurrCamera;
            ObjectA.m_obj.SetActive(true);
            ObjectA.CurrCamera.gameObject.SetActive(true);
            ObjectA.CurrCamera.enabled = true;
        }
        if (ObjectB != null && ObjectB.m_obj != null)
        {
            toCamera = ObjectB.CurrCamera;
            ObjectB.m_obj.SetActive(true);

            ObjectB.CurrCamera.gameObject.SetActive(true);
            ObjectB.CurrCamera.enabled = false;

        }
        if (fromCamera != null && toCamera != null)
        {

            switch (mode)
            {
                case TransitMode.Alpha:

                    CameraTransitionEffectController.Instance.ActivateTransition<AlphaFadeTransitionEffect>(fromCamera, toCamera, Duration, reassignAudioListenerToTargetCamera);

                    break;

                case TransitMode.Deg45:
                    CameraTransitionEffectController.Instance.ActivateTransition<Diagonal45DegTransitionEffect>(fromCamera, toCamera, Duration, reassignAudioListenerToTargetCamera);
                    break;

                case TransitMode.Vertical:
                    CameraTransitionEffectController.Instance.ActivateTransition<VerticalLinesTransitionEffect>(fromCamera, toCamera, Duration, reassignAudioListenerToTargetCamera);

                    break;

                case TransitMode.Diamond:
                    CameraTransitionEffectController.Instance.ActivateTransition<DiamondTransitionEffect>(fromCamera, toCamera, Duration, reassignAudioListenerToTargetCamera);

                    break;
                case TransitMode.Textfade:
                    CameraTransitionEffectController.Instance.ActivateTransition(fromCamera, toCamera, transitionTestMaskTexture, Duration, reassignAudioListenerToTargetCamera);

                    break;

            }

            StartCoroutine(RunOnTransitionFinish(.01f));


        }
        else
        {
            ObjectB.m_obj.SetActive(true);
            ObjectB.CurrCamera.gameObject.SetActive(true);

        }

    }
    IEnumerator RunOnTransitionFinish(float sec)
    {

        yield return new WaitForSeconds(sec);

        if (ObjectA != null && ObjectA.m_obj != null)
        {
            ObjectA.m_obj.SetActive(false);
        }


    }
    public void SetFrameByTime(float ontime)
    {
        if(mode != TransitMode.None)
        {
            SetObectAniFrameByCode(ontime);
            
        }
    }

    void SetObectAniFrameByCode(float ontime)
    {
        if (ObjectB != null && ObjectB.m_obj != null)
        {

            if (ontime > 10)
            {
                ObjectB.m_obj.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                ObjectB.m_obj.transform.localScale = new Vector3(ontime/10, ontime / 10, ontime / 10);
            }
        }
    }


}